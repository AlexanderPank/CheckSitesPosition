using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace CheckPosition
{
    public  class DataBaseSqlite
    {
        private SqliteConnection _connection;
        private String dataBasePath = "";
        public DataBaseSqlite(String curPath)
        {
            dataBasePath = Path.Combine(curPath, "database.db");
            if (File.Exists(dataBasePath))
            {
                string connectionString = $"Data Source={dataBasePath};Mode=ReadWrite;";
                this._connection = new SqliteConnection(connectionString);
                try
                {
                    this._connection.Open();
                    if (this._connection.State != ConnectionState.Open) { MessageBox.Show("Ошибка соединения с БД "); }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка соединения с БД " + ex.Message);
                }

            }
        }
        ~DataBaseSqlite()
        {
            if (this._connection.State == System.Data.ConnectionState.Open)
               try { this._connection.Close(); } catch { }
        }
        public int appendSite(string date, string pageAddress, string Query, int positionCurrent, int positionPrevious, string urlInSearch, string comment, string status) {
     
            string query = $"INSERT INTO sites (date, page_address, query, position_current, position_previous, url_in_search, comment, status) VALUES ";
                   query += $"('{date}', '{pageAddress}', '{Query}', {positionCurrent}, {positionPrevious}, '{urlInSearch}', '{comment}', '{status}')";
            execSQL(query);

            string resutl = execSQL(@"select last_insert_rowid()");

            return resutl == "" ? -1 : int.Parse(resutl);
        }
        private string execSQL(string sql)
        {
            try
            {
                this._connection.Open();
                using (SqliteCommand command = new SqliteCommand(sql, this._connection))
                {
                    // Выполняем SQL-запрос и получаем результат
                    string result = command.ExecuteScalar()?.ToString();
                    // Если результат не равен null, выводим его в месседж бокс
                    if (result != null)
                    {
                        return result;
                    }
                    return "";
                }
                this._connection.Close();
            }
            catch
            {
                MessageBox.Show("Данные не коректны");
            }
            return "";
        }
        public void insertChecks(int site_id, string date, int positionCurrent, int middle_position)
        {
            string query = $"INSERT INTO checks  (date, site_id, position, middle_position) VALUES ('{date}','{site_id}',{positionCurrent}, {middle_position}) ";
            execSQL(query);
        } 
        public void updateSite(int id, string date, string pageAddress, string Query, int positionCurrent, int positionMiddleCurrent, int positionPrevious, int positionMiddlePrevious, string urlInSearch, string comment, string status)
        {
            string query = $"UPDATE sites SET date='{date}', page_address='{pageAddress}', query='{Query}', position_current={positionCurrent}, position_middle_current={positionMiddleCurrent}, " +
                           $"position_previous={positionPrevious}, position_midlle_previous={positionMiddlePrevious}, url_in_search='{urlInSearch}', comment='{comment}', status='{status}' " +
                           $"WHERE id={id}";
            execSQL(query);
        }

        public void removeRecord(int id, string tableName)
        {
            execSQL($"DELETE FROM {tableName} WHERE id={id}");
        }
        public void getTableData(DataGridView dg, string tableName) {
            try
            {
                dg.Rows.Clear();

                DataTable dTable = new DataTable();
                string query = $"SELECT * FROM {tableName} ORDER BY id";

                var command = _connection.CreateCommand();
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        List<string> list = new List<string>(); 
                        for(int i=0;  i<reader.FieldCount; i++)
                        {
                            list.Add(reader.GetString(i));
                        }
                        dg.Rows.Add(list.ToArray());
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

        }

        public int getIdDomainByName(string dname)
        {
            string val = execSQL($"SELECT id FROM domains WHERE name='{dname}'");
            return Helper.getIngValue(val, -1);
        }
        public void appendDomain(long id, string dname, string expiration_date)
        {
            execSQL($"INSERT INTO domains (id, name, expire_date, rus_name, ip, has_site, comments) VALUES ({id}, '{dname}', '{expiration_date}', '', '', '', '')");
        }
        public void updateDomain(long id, string expiration_date)
        {
            execSQL($"UPDATE domains SET expire_date='{expiration_date}'  WHERE id={id};");
        }        
        public void updateRecord( string tableName, long id, string fieldName, string value)
        {
            execSQL($"UPDATE {tableName} SET {fieldName}='{value}' WHERE id={id};");
        }
        public void updateRecord(string tableName, long id, string fieldName, int value)
        {
            execSQL($"UPDATE {tableName} SET {fieldName}={value} WHERE id={id};");
        }

        public List<string> getNotCheckedSiteList()
        {
            List<string> lst = new List<string>();
            DateTime currentDate = DateTime.Now;
            string day = currentDate.Day.ToString("D2");
            string month = currentDate.Month.ToString("D2");
            string year = currentDate.Year.ToString("D2");
            try
            {
                string query1 = $"select name, rus_name, expire_date from domains where expire_date >= '{year}-{month}-{day}' order by expire_date";
                string query2 = $"select page_address from sites";
                List<string> tmp = new List<string>();

                var command = _connection.CreateCommand();
                command.CommandText = query2;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["page_address"].ToString();
                        tmp.Add(name); 
                    }
                }

                command = _connection.CreateCommand();
                command.CommandText = query1;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["name"].ToString();
                        string rusName = BrowserEO.GetDomainName(name); ;
                        if (name == "гостиница-кварт-казань.рф")
                        {
                            name = name;
                        }
                        if (rusName!="")
                        {
                            rusName = rusName.ToLower().Replace("www.", "").Replace("http://", "").Replace("https://", "").Replace("/", "").Trim();
                        }
                        bool b_found = false;
                        for (int i = 0; i < tmp.Count; i++)
                        {
                            string address = tmp[i].ToString().Trim().ToLower();
                            address = address.Replace("www.", "").Replace("http://", "").Replace("https://", "").Replace("/", "").Trim();
                            if (address == name || address == rusName)
                            {
                                    b_found = true;
                                    break;
                            }
                           
                        }
                        if (!b_found) lst.Add(name);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return lst;
        }
    }
}
