using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using DatabaseProject.Models;

namespace DatabaseProject.DAL
{
    public class CustomerDAL
    {
        private readonly string _connectionString;

        public CustomerDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public List<Customer> GetAllCustomers()
        {
            var customers = new List<Customer>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT CustomerID, AccountCode, CompanyName, TaxID, TaxOffice, 
                                 Address, PhoneNumber, RiskLimit, CurrentBalance, AvailableRisk 
                                 FROM Customers ORDER BY CompanyName";
                
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(new Customer
                        {
                            CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                            AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                            CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                            TaxID = reader.IsDBNull(reader.GetOrdinal("TaxID")) ? null : reader.GetString(reader.GetOrdinal("TaxID")),
                            TaxOffice = reader.IsDBNull(reader.GetOrdinal("TaxOffice")) ? null : reader.GetString(reader.GetOrdinal("TaxOffice")),
                            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                            RiskLimit = reader.GetDecimal(reader.GetOrdinal("RiskLimit")),
                            CurrentBalance = reader.GetDecimal(reader.GetOrdinal("CurrentBalance"))
                        });
                    }
                }
            }

            return customers;
        }

        public (int SuccessCount, int ErrorCount, List<string> Errors) ParseAndImportReport(string fileContent)
        {
            int successCount = 0;
            int errorCount = 0;
            var errors = new List<string>();

            var chunks = Regex.Split(fileContent, @"(?=Cari\s*Kodu)", RegexOptions.IgnoreCase);
            
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                int chunkIndex = 0;

                foreach (var chunk in chunks)
                {
                    chunkIndex++;
                    if (string.IsNullOrWhiteSpace(chunk)) continue;
                    if (!chunk.Trim().StartsWith("Cari", StringComparison.OrdinalIgnoreCase)) continue;

                    try
                    {
                        var customer = ExtractCustomerFromChunk(chunk);
                        
                        if (customer == null || string.IsNullOrWhiteSpace(customer.AccountCode))
                        {
                            continue;
                        }

                        SaveCustomerDirect(conn, customer);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"Blok {chunkIndex}: {ex.Message}");
                    }
                }
            }

            return (successCount, errorCount, errors);
        }

        private Customer? ExtractCustomerFromChunk(string chunk)
        {
            var customer = new Customer();

            var codeMatch = Regex.Match(chunk, @"Cari\s*Kodu\s*[:\s]*([A-Za-z0-9\-\.]+)", RegexOptions.IgnoreCase);
            if (codeMatch.Success)
            {
                customer.AccountCode = codeMatch.Groups[1].Value.Trim();
            }
            else
            {
                return null;
            }

            var nameMatch = Regex.Match(chunk, @"Cari\s*Ad[ıi]\s*[:\s]*(.+?)(?=\s*(?:Alacak|Borç|Telefon|Adres|Risk|$))", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (nameMatch.Success)
            {
                customer.CompanyName = CleanText(nameMatch.Groups[1].Value);
            }
            else
            {
                var altNameMatch = Regex.Match(chunk, @"Cari\s*Kodu\s*[:\s]*[A-Za-z0-9\-\.]+\s+(.+?)(?=\s*\n)", RegexOptions.IgnoreCase);
                if (altNameMatch.Success)
                {
                    customer.CompanyName = CleanText(altNameMatch.Groups[1].Value);
                }
            }

            if (string.IsNullOrWhiteSpace(customer.CompanyName))
            {
                customer.CompanyName = "Bilinmeyen Firma";
            }

            var phoneMatch = Regex.Match(chunk, @"Telefon\s*[:\s]*([\d\s\-\(\)]+?)(?=\s*(?:Borç|Adres|Fax|$))", RegexOptions.IgnoreCase);
            if (phoneMatch.Success)
            {
                var phone = Regex.Replace(phoneMatch.Groups[1].Value, @"[^\d]", "");
                if (phone.Length >= 7 && phone.Length <= 15)
                {
                    customer.PhoneNumber = FormatPhoneNumber(phone);
                }
            }

            var addressMatch = Regex.Match(chunk, @"Adres\s*[:\s]*(.+?)(?=\s*(?:#|Belge|Tarih|Borç|Telefon|\d{2}\.\d{2}\.\d{4}|$))", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (addressMatch.Success)
            {
                customer.Address = CleanText(addressMatch.Groups[1].Value);
            }

            var taxMatch = Regex.Match(chunk, @"Vergi\s*(?:No|Numarası|Kimlik)\s*[:\s]*(\d{10,11})", RegexOptions.IgnoreCase);
            if (taxMatch.Success)
            {
                customer.TaxID = taxMatch.Groups[1].Value.Trim();
            }

            var taxOfficeMatch = Regex.Match(chunk, @"Vergi\s*Dairesi\s*[:\s]*([^\n\r]+)", RegexOptions.IgnoreCase);
            if (taxOfficeMatch.Success)
            {
                customer.TaxOffice = CleanText(taxOfficeMatch.Groups[1].Value);
            }

            customer.CurrentBalance = CalculateBalance(chunk);
            customer.RiskLimit = 0;

            return customer;
        }

        private decimal CalculateBalance(string chunk)
        {
            decimal borcBakiye = 0;
            decimal alacakBakiye = 0;

            var borcMatch = Regex.Match(chunk, @"Borç\s*Bakiye\s*[:\s]*([\d\.,\s]+)", RegexOptions.IgnoreCase);
            if (borcMatch.Success)
            {
                borcBakiye = ParseTurkishDecimal(borcMatch.Groups[1].Value);
            }

            var alacakMatch = Regex.Match(chunk, @"Alacak\s*Bakiye\s*[:\s]*([\d\.,\s]+)", RegexOptions.IgnoreCase);
            if (alacakMatch.Success)
            {
                alacakBakiye = ParseTurkishDecimal(alacakMatch.Groups[1].Value);
            }

            return borcBakiye - alacakBakiye;
        }

        private decimal ParseTurkishDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            value = value.Trim().Replace(" ", "");
            value = value.Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return 0;
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            text = Regex.Replace(text, @"[\r\n\t]+", " ");
            text = Regex.Replace(text, @"\s{2,}", " ");
            text = text.Replace("'", "''");
            return text.Trim();
        }

        private string FormatPhoneNumber(string digits)
        {
            if (digits.Length == 10)
                return $"0{digits.Substring(0,3)} {digits.Substring(3,3)} {digits.Substring(6,4)}";
            if (digits.Length == 11)
                return $"{digits.Substring(0,4)} {digits.Substring(4,3)} {digits.Substring(7,4)}";
            return digits;
        }

        private void SaveCustomerDirect(SqlConnection conn, Customer customer)
        {
            string checkQuery = "SELECT COUNT(*) FROM Customers WHERE AccountCode = @AccountCode";
            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@AccountCode", customer.AccountCode);
                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    string updateQuery = @"UPDATE Customers SET 
                                           CompanyName = @CompanyName,
                                           TaxID = @TaxID,
                                           TaxOffice = @TaxOffice,
                                           Address = @Address,
                                           PhoneNumber = @PhoneNumber,
                                           CurrentBalance = @CurrentBalance
                                           WHERE AccountCode = @AccountCode";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        AddCustomerParameters(cmd, customer);
                        cmd.Parameters.AddWithValue("@CurrentBalance", customer.CurrentBalance);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string insertQuery = @"INSERT INTO Customers 
                                          (AccountCode, CompanyName, TaxID, TaxOffice, Address, PhoneNumber, RiskLimit, CurrentBalance) 
                                          VALUES 
                                          (@AccountCode, @CompanyName, @TaxID, @TaxOffice, @Address, @PhoneNumber, @RiskLimit, @CurrentBalance)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        AddCustomerParameters(cmd, customer);
                        cmd.Parameters.AddWithValue("@RiskLimit", customer.RiskLimit);
                        cmd.Parameters.AddWithValue("@CurrentBalance", customer.CurrentBalance);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void AddCustomerParameters(SqlCommand cmd, Customer customer)
        {
            cmd.Parameters.AddWithValue("@AccountCode", customer.AccountCode);
            cmd.Parameters.AddWithValue("@CompanyName", customer.CompanyName);
            cmd.Parameters.AddWithValue("@TaxID", (object?)customer.TaxID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TaxOffice", (object?)customer.TaxOffice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)customer.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)customer.PhoneNumber ?? DBNull.Value);
        }
    }
}
