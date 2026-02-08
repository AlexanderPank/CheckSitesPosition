using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using PsiMetricsNet48;
using WordStatisticParserClient;

namespace CheckPosition
{
    public  class DataBaseSqlite
    {
        private SqliteConnection _connection;
        private String dataBasePath = "";
        // Добавляем приватный объект для синхронизации обращений к БД
        private readonly object _dbSync = new object();
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
                    // Проверяем и создаем таблицу аналитики, если ее еще нет
                    EnsureSiteAnalysisTableExists();
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
        // Добавляем новый сайт с начальными данными о позиции и конкуренте
        public int appendSite(string date, string pageAddress, string Query, int positionCurrent, int positionPrevious, string urlInSearch, string competitor, string comment, string status) {
     
            string query = $"INSERT INTO sites (date, page_address, query, position_current, position_previous, url_in_search, competitor, comment, status) VALUES ";
                   query += $"('{date}', '{pageAddress}', '{Query}', {positionCurrent}, {positionPrevious}, '{urlInSearch}', '{competitor}', '{comment}', '{status}')";
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
        // Возвращает историю изменения позиций сайта из таблицы checks
        public IReadOnlyList<CheckHistoryPoint> GetCheckHistory(int siteId)
        {
            // Формируем список точек истории с учетом потоко-безопасности
            if (siteId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(siteId));
            }

            // Создаем список для накопления результатов выгрузки
            var history = new List<CheckHistoryPoint>();

            lock (_dbSync)
            {
                // Выполняем параметризованный запрос, чтобы исключить SQL-инъекции
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = "SELECT date, position, middle_position FROM checks WHERE site_id = $siteId ORDER BY date;";
                    command.Parameters.AddWithValue("$siteId", siteId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Парсим дату в безопасном режиме, сохраняя исходные данные при ошибке
                            string rawDate = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            DateTime parsedDate;
                            bool parsed = DateTime.TryParse(rawDate, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out parsedDate) ||
                                          DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsedDate);
                            if (!parsed)
                            {
                                // Пропускаем запись, если дату корректно распознать не удалось
                                continue;
                            }

                            // Забираем позиции с учетом возможных NULL в базе
                            int position = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            int? middlePosition = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);

                            history.Add(new CheckHistoryPoint(parsedDate, position, middlePosition));
                        }
                    }
                }
            }

            return history;
        }
        // Обновляем данные сайта, включая информацию о конкурентах
        public void updateSite(int id, string date, string pageAddress, string Query, int positionCurrent, int positionMiddleCurrent, int positionPrevious, int positionMiddlePrevious, string urlInSearch, string competitor, string comment, string status)
        {
            string query = $"UPDATE sites SET date='{date}', page_address='{pageAddress}', query='{Query}', position_current={positionCurrent}, position_middle_current={positionMiddleCurrent}, " +
                           $"position_previous={positionPrevious}, position_midlle_previous={positionMiddlePrevious}, url_in_search='{urlInSearch}', competitor='{competitor}', comment='{comment}', status='{status}' " +
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
                // Формируем запрос с учетом необходимости подтянуть название хостинга для таблицы сайтов
                dg.Rows.Clear();
                string query;
                if (string.Equals(tableName, "sites", StringComparison.OrdinalIgnoreCase))
                {
                    // Формируем запрос с дополнительными сведениями о CPA и хостинге для таблицы сайтов
                    query = "SELECT s.id, s.date, s.page_address, s.query, s.position_current, s.position_middle_current, " +
                            "s.position_previous, s.position_midlle_previous, s.url_in_search, s.competitor, s.comment, s.status, " +
                            "COALESCE(s.cpa_id, 0) AS cpa_id, IFNULL(c.name, '') AS cpa_name, " +
                            "COALESCE(s.hosting_id, 0) AS hosting_id, IFNULL(h.name, '') AS hosting_name FROM sites s " +
                            "LEFT JOIN cpa_list c ON c.id = s.cpa_id " +
                            "LEFT JOIN hosting_list h ON h.id = s.hosting_id ORDER BY s.id;";
                }
                else
                {
                    query = $"SELECT * FROM {tableName} ORDER BY id";
                }

                lock (_dbSync)
                {
                    EnsureConnectionOpen();
                    using (var command = _connection.CreateCommand())
                    {
                        command.CommandText = query;
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Собираем значения строки, корректно обрабатывая пустые значения
                                List<string> list = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    list.Add(reader.IsDBNull(i) ? string.Empty : reader.GetValue(i).ToString());
                                }
                                dg.Rows.Add(list.ToArray());
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

        }

        // Загружаем домены с IP-адресами для определения хостинга
        public List<DomainRecord> LoadDomainsWithIp()
        {
            var result = new List<DomainRecord>();
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, name, rus_name, ip FROM domains ORDER BY id;";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                            string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            string rusName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            string ip = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                            result.Add(new DomainRecord(id, name, rusName, ip));
                        }
                    }
                }
            }
            return result;
        }

        // Загружаем список сайтов для которых требуется определить хостинг
        public List<SiteHostingCandidate> LoadSitesForHostingDetection(IReadOnlyCollection<long> siteIds)
        {
            var result = new List<SiteHostingCandidate>();
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    if (siteIds != null && siteIds.Count > 0)
                    {
                        // Готовим параметризованный список идентификаторов для выборки
                        int index = 0;
                        var placeholders = new List<string>();
                        foreach (var siteId in siteIds)
                        {
                            string parameterName = "$id" + index++;
                            placeholders.Add(parameterName);
                            command.Parameters.AddWithValue(parameterName, siteId);
                        }
                        string inClause = string.Join(",", placeholders);
                        command.CommandText = $"SELECT id, page_address, hosting_id FROM sites WHERE id IN ({inClause}) ORDER BY id;";
                    }
                    else
                    {
                        command.CommandText = "SELECT id, page_address, hosting_id FROM sites ORDER BY id;";
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                            string pageAddress = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            long hostingId = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                            result.Add(new SiteHostingCandidate(id, pageAddress, hostingId));
                        }
                    }
                }
            }
            return result;
        }

        // Загружаем список сайтов для определения CPA-скриптов
        public List<SiteCpaCandidate> LoadSitesForCpaDetection(IReadOnlyCollection<long> siteIds)
        {
            var result = new List<SiteCpaCandidate>();
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    if (siteIds != null && siteIds.Count > 0)
                    {
                        // Формируем параметризованный список идентификаторов сайтов для выборки по CPA
                        int index = 0;
                        var placeholders = new List<string>();
                        foreach (var siteId in siteIds)
                        {
                            string parameterName = "$cpaId" + index++;
                            placeholders.Add(parameterName);
                            command.Parameters.AddWithValue(parameterName, siteId);
                        }
                        string inClause = string.Join(",", placeholders);
                        command.CommandText = $"SELECT id, page_address, COALESCE(cpa_id, 0) FROM sites WHERE id IN ({inClause}) ORDER BY id;";
                    }
                    else
                    {
                        command.CommandText = "SELECT id, page_address, COALESCE(cpa_id, 0) FROM sites ORDER BY id;";
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                            string pageAddress = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            long cpaId = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                            result.Add(new SiteCpaCandidate(id, pageAddress, cpaId));
                        }
                    }
                }
            }
            return result;
        }

        // Обновляем хостинг для множества сайтов в рамках одной транзакции
        public void UpdateSiteHostingBulk(IReadOnlyDictionary<long, long> updates)
        {
            if (updates == null || updates.Count == 0) return;

            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var transaction = _connection.BeginTransaction())
                using (var command = _connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "UPDATE sites SET hosting_id = $hostingId WHERE id = $id;";
                    var idParameter = command.CreateParameter();
                    idParameter.ParameterName = "$id";
                    command.Parameters.Add(idParameter);
                    var hostingParameter = command.CreateParameter();
                    hostingParameter.ParameterName = "$hostingId";
                    command.Parameters.Add(hostingParameter);

                    foreach (var update in updates)
                    {
                        idParameter.Value = update.Key;
                        hostingParameter.Value = update.Value;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        // Обновляем CPA для набора сайтов атомарно
        public void UpdateSiteCpaBulk(IReadOnlyDictionary<long, long> updates)
        {
            if (updates == null || updates.Count == 0) return;

            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var transaction = _connection.BeginTransaction())
                using (var command = _connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "UPDATE sites SET cpa_id = $cpaId WHERE id = $id;";
                    var idParameter = command.CreateParameter();
                    idParameter.ParameterName = "$id";
                    command.Parameters.Add(idParameter);
                    var cpaParameter = command.CreateParameter();
                    cpaParameter.ParameterName = "$cpaId";
                    command.Parameters.Add(cpaParameter);

                    foreach (var update in updates)
                    {
                        idParameter.Value = update.Key;
                        cpaParameter.Value = update.Value;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        // Добавляем метод точечного обновления хостинга сайта с безопасными параметрами
        public void UpdateSiteHosting(long siteId, long? hostingId)
        {
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = "UPDATE sites SET hosting_id = $hostingId WHERE id = $siteId;";
                    command.Parameters.AddWithValue("$siteId", siteId);
                    var hostingParameter = command.CreateParameter();
                    hostingParameter.ParameterName = "$hostingId";
                    hostingParameter.Value = hostingId.HasValue && hostingId.Value > 0 ? (object)hostingId.Value : DBNull.Value;
                    command.Parameters.Add(hostingParameter);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Добавляем метод точечного обновления CPA сайта с безопасными параметрами
        public void UpdateSiteCpa(long siteId, long? cpaId)
        {
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = "UPDATE sites SET cpa_id = $cpaId WHERE id = $siteId;";
                    command.Parameters.AddWithValue("$siteId", siteId);
                    var cpaParameter = command.CreateParameter();
                    cpaParameter.ParameterName = "$cpaId";
                    cpaParameter.Value = cpaId.HasValue && cpaId.Value > 0 ? (object)cpaId.Value : DBNull.Value;
                    command.Parameters.Add(cpaParameter);
                    command.ExecuteNonQuery();
                }
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

        // Добавляем защищенный метод для проверки и открытия соединения
        private void EnsureConnectionOpen()
        {
            lock (_dbSync)
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
            }
        }

        // Проверяем, что таблица аналитики существует, и создаем ее при необходимости
        private void EnsureSiteAnalysisTableExists()
        {
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText =
                        "CREATE TABLE IF NOT EXISTS site_analysis_data (" +
                        "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "site_id INTEGER NOT NULL, " +
                        "check_date TEXT NOT NULL, " +
                        "page_url TEXT, " +
                        "strategy TEXT, " +
                        "fetch_time TEXT, " +
                        "psi_perf_score REAL, " +
                        "psi_seo_score REAL, " +
                        "psi_bp_score REAL, " +
                        "psi_a11y_score REAL, " +
                        "psi_lcp_ms REAL, " +
                        "psi_cls REAL, " +
                        "psi_inp_ms REAL, " +
                        "psi_tbt_ms REAL, " +
                        "psi_ttfb_ms REAL, " +
                        "psi_fcp_ms REAL, " +
                        "psi_si_ms REAL, " +
                        "psi_bytes REAL, " +
                        "psi_req_cnt INTEGER, " +
                        "psi_unused_js_b REAL, " +
                        "psi_unused_css_b REAL, " +
                        "psi_offscr_img_b REAL, " +
                        "psi_modern_img_b REAL, " +
                        "psi_opt_img_b REAL, " +
                        "word_keyword TEXT, " +
                        "word_total_words INTEGER, " +
                        "word_total_sentences INTEGER, " +
                        "word_total_paragraphs INTEGER, " +
                        "word_total_words_in_paragraphs INTEGER, " +
                        "word_h1_count INTEGER, " +
                        "word_h2_count INTEGER, " +
                        "word_h3_count INTEGER, " +
                        "word_h4_count INTEGER, " +
                        "word_h5_count INTEGER, " +
                        "word_total_words_in_headers INTEGER, " +
                        "word_total_words_in_title INTEGER, " +
                        "word_total_words_in_description INTEGER, " +
                        "word_image_count INTEGER, " +
                        "word_inner_links INTEGER, " +
                        "word_outer_links INTEGER, " +
                        "word_total_words_in_links INTEGER, " +
                        "word_kw_words_count INTEGER, " +
                        "word_kw_words_in_title INTEGER, " +
                        "word_kw_words_in_description INTEGER, " +
                        "word_kw_words_in_headers INTEGER, " +
                        "word_kw_words_in_alt INTEGER, " +
                        "word_kw_words_in_text INTEGER, " +
                        "word_tokens_ratio REAL, " +
                        "word_kincaid_score REAL, " +
                        "word_flesch_reading_ease REAL, " +
                        "word_gunning_fog REAL, " +
                        "word_smog_index REAL, " +
                        "word_ari REAL, " +
                        "word_main_keyword_density REAL, " +
                        "raw_json TEXT" +
                        ");";
                    command.ExecuteNonQuery();
                }

                using (var indexCommand = _connection.CreateCommand())
                {
                    indexCommand.CommandText = "CREATE INDEX IF NOT EXISTS idx_site_analysis_site_id ON site_analysis_data(site_id);";
                    indexCommand.ExecuteNonQuery();
                }
            }
        }

        // Загружаем список сайтов для анализа (URL и ключевая фраза)
        public List<SiteParserTarget> LoadSitesForAnalysis(List<long> siteIds)
        {
            var result = new List<SiteParserTarget>();
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    if (siteIds != null && siteIds.Count > 0)
                    {
                        var parameterNames = new List<string>();
                        int index = 0;
                        foreach (long siteId in siteIds)
                        {
                            string parameterName = "$id" + index;
                            parameterNames.Add(parameterName);
                            command.Parameters.AddWithValue(parameterName, siteId);
                            index++;
                        }

                        command.CommandText = $"SELECT id, page_address, query FROM sites WHERE id IN ({string.Join(", ", parameterNames)}) ORDER BY id;";
                    }
                    else
                    {
                        command.CommandText = "SELECT id, page_address, query FROM sites ORDER BY id;";
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                            string pageAddress = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            string query = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            result.Add(new SiteParserTarget(id,  pageAddress, query));
                        }
                    }
                }
            }
            return result;
        }

        // Загружаем данные аналитики с подстановкой позиции сайта вместо адреса
        public DataTable LoadAnalysisData(string strategyFilter)
        {
            var table = new DataTable();
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                // Приводим таблицу аналитики к правилу "две записи на сайт", чтобы избежать дублей
                NormalizeSiteAnalysisRecords();
                using (var command = _connection.CreateCommand())
                {
                    // Формируем запрос с нужным порядком колонок, чтобы первые поля совпадали с отображением в форме
                    string baseQuery =
                        "SELECT a.id, a.site_id, a.page_url, s.position_current, a.check_date, a.strategy, a.fetch_time, " +
                        "a.psi_perf_score, a.psi_seo_score, a.psi_bp_score, a.psi_a11y_score, a.psi_lcp_ms, a.psi_cls, " +
                        "a.psi_inp_ms, a.psi_tbt_ms, a.psi_ttfb_ms, a.psi_fcp_ms, a.psi_si_ms, a.psi_bytes, a.psi_req_cnt, " +
                        "a.psi_unused_js_b, a.psi_unused_css_b, a.psi_offscr_img_b, a.psi_modern_img_b, a.psi_opt_img_b, " +
                        "a.word_keyword, a.word_total_words, a.word_total_sentences, a.word_total_paragraphs, a.word_total_words_in_paragraphs, " +
                        "a.word_h1_count, a.word_h2_count, a.word_h3_count, a.word_h4_count, a.word_h5_count, a.word_total_words_in_headers, " +
                        "a.word_total_words_in_title, a.word_total_words_in_description, a.word_image_count, a.word_inner_links, a.word_outer_links, " +
                        "a.word_total_words_in_links, a.word_kw_words_count, a.word_kw_words_in_title, a.word_kw_words_in_description, " +
                        "a.word_kw_words_in_headers, a.word_kw_words_in_alt, a.word_kw_words_in_text, a.word_tokens_ratio, a.word_kincaid_score, " +
                        "a.word_flesch_reading_ease, a.word_gunning_fog, a.word_smog_index, a.word_ari, a.word_main_keyword_density, a.raw_json " +
                        "FROM site_analysis_data a " +
                        "LEFT JOIN sites s ON s.id = a.site_id ";

                    if (!string.IsNullOrWhiteSpace(strategyFilter))
                    {
                        // Фильтруем данные по стратегии, чтобы в таблице оставалась одна запись на сайт
                        baseQuery += "WHERE a.strategy = $strategy ";
                        command.Parameters.AddWithValue("$strategy", strategyFilter);
                    }

                    command.CommandText = baseQuery + "ORDER BY a.id DESC;";
                    using (var reader = command.ExecuteReader())
                    {
                        table.Load(reader);
                    }
                }
            }
            return table;
        }

        // Добавляем пустые записи аналитики для сайтов, которых нет в таблице site_analysis_data по каждой стратегии
        public int InsertMissingAnalysisRecords()
        {
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var transaction = _connection.BeginTransaction())
                {
                    int inserted = 0;
                    string checkDate = DateTimeOffset.Now.ToString("O");
                    using (var command = _connection.CreateCommand())
                    {
                        // Формируем массовую вставку с дефолтными значениями, чтобы избежать циклов и лишних запросов
                        command.Transaction = transaction;
                        command.CommandText =
                            "INSERT INTO site_analysis_data (" +
                            "site_id, check_date, page_url, strategy, fetch_time, " +
                            "psi_perf_score, psi_seo_score, psi_bp_score, psi_a11y_score, psi_lcp_ms, psi_cls, psi_inp_ms, psi_tbt_ms, " +
                            "psi_ttfb_ms, psi_fcp_ms, psi_si_ms, psi_bytes, psi_req_cnt, psi_unused_js_b, psi_unused_css_b, " +
                            "psi_offscr_img_b, psi_modern_img_b, psi_opt_img_b, " +
                            "word_keyword, word_total_words, word_total_sentences, word_total_paragraphs, word_total_words_in_paragraphs, " +
                            "word_h1_count, word_h2_count, word_h3_count, word_h4_count, word_h5_count, word_total_words_in_headers, " +
                            "word_total_words_in_title, word_total_words_in_description, word_image_count, word_inner_links, word_outer_links, " +
                            "word_total_words_in_links, word_kw_words_count, word_kw_words_in_title, word_kw_words_in_description, " +
                            "word_kw_words_in_headers, word_kw_words_in_alt, word_kw_words_in_text, word_tokens_ratio, word_kincaid_score, " +
                            "word_flesch_reading_ease, word_gunning_fog, word_smog_index, word_ari, word_main_keyword_density, raw_json" +
                            ") " +
                            "SELECT s.id, $checkDate, s.page_address, $strategy, '', " +
                            "-1, -1, -1, -1, -1, -1, -1, -1, " +
                            "-1, -1, -1, -1, -1, -1, -1, " +
                            "-1, -1, -1, " +
                            "'', -1, -1, -1, -1, " +
                            "-1, -1, -1, -1, -1, -1, " +
                            "-1, -1, -1, -1, -1, " +
                            "-1, -1, -1, " +
                            "-1, -1, -1, -1, -1, -1, " +
                            "-1, -1, -1, -1, -1, '' " +
                            "FROM sites s " +
                            "WHERE NOT EXISTS (SELECT 1 FROM site_analysis_data a WHERE a.site_id = s.id AND a.strategy = $strategy);";
                        // Добавляем записи для mobile-стратегии
                        command.Parameters.AddWithValue("$checkDate", checkDate);
                        command.Parameters.AddWithValue("$strategy", "Mobile");
                        inserted += command.ExecuteNonQuery();

                        // Добавляем записи для desktop-стратегии
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("$checkDate", checkDate);
                        command.Parameters.AddWithValue("$strategy", "Desktop");
                        inserted += command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return inserted;
                }
            }
        }

        // Обновляем запись аналитики по результатам парсеров и возвращаем признак успешного обновления
        public bool UpdateSiteAnalysisRecord(long siteId, string pageUrl, string keyword, DateTimeOffset checkDate, PageSpeedMetrics psiMetrics, ParsedPageMetrics wordMetrics)
        {
            if (siteId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(siteId));
            }

            if (psiMetrics == null)
            {
                throw new ArgumentNullException(nameof(psiMetrics));
            }

            // Готовим значения для безопасного обновления с учетом возможных null в метриках
            object DbValue(string value) => string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;
            object DbValueDouble(double? value) => value.HasValue ? (object)value.Value : DBNull.Value;
            object DbValueInt(int? value) => value.HasValue ? (object)value.Value : DBNull.Value;
            object DbValueDate(DateTimeOffset? value) => value.HasValue ? (object)value.Value.ToString("O") : DBNull.Value;

            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE site_analysis_data SET " +
                        "check_date = $checkDate, page_url = $pageUrl, strategy = $strategy, fetch_time = $fetchTime, " +
                        "psi_perf_score = $psiPerfScore, psi_seo_score = $psiSeoScore, psi_bp_score = $psiBpScore, psi_a11y_score = $psiA11yScore, " +
                        "psi_lcp_ms = $psiLcpMs, psi_cls = $psiCls, psi_inp_ms = $psiInpMs, psi_tbt_ms = $psiTbtMs, psi_ttfb_ms = $psiTtfbMs, " +
                        "psi_fcp_ms = $psiFcpMs, psi_si_ms = $psiSiMs, psi_bytes = $psiBytes, psi_req_cnt = $psiReqCnt, " +
                        "psi_unused_js_b = $psiUnusedJsB, psi_unused_css_b = $psiUnusedCssB, psi_offscr_img_b = $psiOffscrImgB, " +
                        "psi_modern_img_b = $psiModernImgB, psi_opt_img_b = $psiOptImgB, " +
                        "word_keyword = $wordKeyword, word_total_words = $wordTotalWords, word_total_sentences = $wordTotalSentences, " +
                        "word_total_paragraphs = $wordTotalParagraphs, word_total_words_in_paragraphs = $wordTotalWordsInParagraphs, " +
                        "word_h1_count = $wordH1Count, word_h2_count = $wordH2Count, word_h3_count = $wordH3Count, word_h4_count = $wordH4Count, " +
                        "word_h5_count = $wordH5Count, word_total_words_in_headers = $wordTotalWordsInHeaders, " +
                        "word_total_words_in_title = $wordTotalWordsInTitle, word_total_words_in_description = $wordTotalWordsInDescription, " +
                        "word_image_count = $wordImageCount, word_inner_links = $wordInnerLinks, word_outer_links = $wordOuterLinks, " +
                        "word_total_words_in_links = $wordTotalWordsInLinks, word_kw_words_count = $wordKwWordsCount, " +
                        "word_kw_words_in_title = $wordKwWordsInTitle, word_kw_words_in_description = $wordKwWordsInDescription, " +
                        "word_kw_words_in_headers = $wordKwWordsInHeaders, word_kw_words_in_alt = $wordKwWordsInAlt, " +
                        "word_kw_words_in_text = $wordKwWordsInText, word_tokens_ratio = $wordTokensRatio, word_kincaid_score = $wordKincaidScore, " +
                        "word_flesch_reading_ease = $wordFleschReadingEase, word_gunning_fog = $wordGunningFog, " +
                        "word_smog_index = $wordSmogIndex, word_ari = $wordAri, word_main_keyword_density = $wordMainKeywordDensity, " +
                        "raw_json = $rawJson " +
                        "WHERE site_id = $siteId AND strategy = $strategy;";

                    command.Parameters.AddWithValue("$siteId", siteId);
                    command.Parameters.AddWithValue("$checkDate", checkDate.ToString("O"));
                    command.Parameters.AddWithValue("$pageUrl", DbValue(pageUrl));
                    command.Parameters.AddWithValue("$strategy", DbValue(psiMetrics.Strategy.ToString()));
                    command.Parameters.AddWithValue("$fetchTime", DbValueDate(psiMetrics.FetchTime));
                    command.Parameters.AddWithValue("$psiPerfScore", DbValueDouble(psiMetrics.PerformanceScore));
                    command.Parameters.AddWithValue("$psiSeoScore", DbValueDouble(psiMetrics.SeoScore));
                    command.Parameters.AddWithValue("$psiBpScore", DbValueDouble(psiMetrics.BestPracticesScore));
                    command.Parameters.AddWithValue("$psiA11yScore", DbValueDouble(psiMetrics.AccessibilityScore));
                    command.Parameters.AddWithValue("$psiLcpMs", DbValueDouble(psiMetrics.LargestContentfulPaintMs));
                    command.Parameters.AddWithValue("$psiCls", DbValueDouble(psiMetrics.CumulativeLayoutShift));
                    command.Parameters.AddWithValue("$psiInpMs", DbValueDouble(psiMetrics.InteractionToNextPaintMs));
                    command.Parameters.AddWithValue("$psiTbtMs", DbValueDouble(psiMetrics.TotalBlockingTimeMs));
                    command.Parameters.AddWithValue("$psiTtfbMs", DbValueDouble(psiMetrics.ServerResponseTimeMs));
                    command.Parameters.AddWithValue("$psiFcpMs", DbValueDouble(psiMetrics.FirstContentfulPaintMs));
                    command.Parameters.AddWithValue("$psiSiMs", DbValueDouble(psiMetrics.SpeedIndexMs));
                    command.Parameters.AddWithValue("$psiBytes", DbValueDouble(psiMetrics.TotalByteWeight));
                    command.Parameters.AddWithValue("$psiReqCnt", DbValueInt(psiMetrics.NetworkRequestsCount));
                    command.Parameters.AddWithValue("$psiUnusedJsB", DbValueDouble(psiMetrics.UnusedJavaScriptSavingsBytes));
                    command.Parameters.AddWithValue("$psiUnusedCssB", DbValueDouble(psiMetrics.UnusedCssSavingsBytes));
                    command.Parameters.AddWithValue("$psiOffscrImgB", DbValueDouble(psiMetrics.OffscreenImagesSavingsBytes));
                    command.Parameters.AddWithValue("$psiModernImgB", DbValueDouble(psiMetrics.ModernImageFormatsSavingsBytes));
                    command.Parameters.AddWithValue("$psiOptImgB", DbValueDouble(psiMetrics.UsesOptimizedImagesSavingsBytes));

                    // Используем ключевую фразу из метрик, а при отсутствии берем исходный запрос сайта
                    string effectiveKeyword = !string.IsNullOrWhiteSpace(wordMetrics?.Keyword) ? wordMetrics.Keyword : keyword;
                    command.Parameters.AddWithValue("$wordKeyword", DbValue(effectiveKeyword));
                    command.Parameters.AddWithValue("$wordTotalWords", DbValueInt(wordMetrics?.TotalWords));
                    command.Parameters.AddWithValue("$wordTotalSentences", DbValueInt(wordMetrics?.TotalSentences));
                    command.Parameters.AddWithValue("$wordTotalParagraphs", DbValueInt(wordMetrics?.TotalParagraphs));
                    command.Parameters.AddWithValue("$wordTotalWordsInParagraphs", DbValueInt(wordMetrics?.TotalWordsInParagraphs));
                    command.Parameters.AddWithValue("$wordH1Count", DbValueInt(wordMetrics?.H1Count));
                    command.Parameters.AddWithValue("$wordH2Count", DbValueInt(wordMetrics?.H2Count));
                    command.Parameters.AddWithValue("$wordH3Count", DbValueInt(wordMetrics?.H3Count));
                    command.Parameters.AddWithValue("$wordH4Count", DbValueInt(wordMetrics?.H4Count));
                    command.Parameters.AddWithValue("$wordH5Count", DbValueInt(wordMetrics?.H5Count));
                    command.Parameters.AddWithValue("$wordTotalWordsInHeaders", DbValueInt(wordMetrics?.TotalWordsInHeaders));
                    command.Parameters.AddWithValue("$wordTotalWordsInTitle", DbValueInt(wordMetrics?.TotalWordsInTitle));
                    command.Parameters.AddWithValue("$wordTotalWordsInDescription", DbValueInt(wordMetrics?.TotalWordsInDescription));
                    command.Parameters.AddWithValue("$wordImageCount", DbValueInt(wordMetrics?.ImageCount));
                    command.Parameters.AddWithValue("$wordInnerLinks", DbValueInt(wordMetrics?.InnerLinks));
                    command.Parameters.AddWithValue("$wordOuterLinks", DbValueInt(wordMetrics?.OuterLinks));
                    command.Parameters.AddWithValue("$wordTotalWordsInLinks", DbValueInt(wordMetrics?.TotalWordsInLinks));
                    command.Parameters.AddWithValue("$wordKwWordsCount", DbValueInt(wordMetrics?.KeywordWordsCount));
                    command.Parameters.AddWithValue("$wordKwWordsInTitle", DbValueInt(wordMetrics?.KeywordWordsInTitle));
                    command.Parameters.AddWithValue("$wordKwWordsInDescription", DbValueInt(wordMetrics?.KeywordWordsInDescription));
                    command.Parameters.AddWithValue("$wordKwWordsInHeaders", DbValueInt(wordMetrics?.KeywordWordsInHeaders));
                    command.Parameters.AddWithValue("$wordKwWordsInAlt", DbValueInt(wordMetrics?.KeywordWordsInAlt));
                    command.Parameters.AddWithValue("$wordKwWordsInText", DbValueInt(wordMetrics?.KeywordWordsInText));
                    command.Parameters.AddWithValue("$wordTokensRatio", DbValueDouble(wordMetrics?.TokensRatio));
                    command.Parameters.AddWithValue("$wordKincaidScore", DbValueDouble(wordMetrics?.KincaidScore));
                    command.Parameters.AddWithValue("$wordFleschReadingEase", DbValueDouble(wordMetrics?.FleschReadingEase));
                    command.Parameters.AddWithValue("$wordGunningFog", DbValueDouble(wordMetrics?.GunningFog));
                    command.Parameters.AddWithValue("$wordSmogIndex", DbValueDouble(wordMetrics?.SmogIndex));
                    command.Parameters.AddWithValue("$wordAri", DbValueDouble(wordMetrics?.AutomatedReadabilityIndex));
                    command.Parameters.AddWithValue("$wordMainKeywordDensity", DbValueDouble(wordMetrics?.MainKeywordDensity));
                    command.Parameters.AddWithValue("$rawJson", DbValue(wordMetrics?.RawJson));

                    int affected = command.ExecuteNonQuery();
                    return affected > 0;
                }
            }
        }

        // Нормализуем таблицу аналитики, оставляя по одной записи на стратегию для каждого сайта
        private void NormalizeSiteAnalysisRecords()
        {
            using (var command = _connection.CreateCommand())
            {
                // Удаляем записи со стратегией вне списка, чтобы оставить только mobile и desktop
                command.CommandText =
                    "DELETE FROM site_analysis_data WHERE strategy IS NULL OR TRIM(strategy) = '' OR strategy NOT IN ('Mobile', 'Desktop');";
                command.ExecuteNonQuery();

                // Удаляем дубликаты, сохраняя по одной записи на стратегию и сайт
                command.CommandText =
                    "DELETE FROM site_analysis_data " +
                    "WHERE id NOT IN (" +
                    "SELECT MAX(id) FROM site_analysis_data GROUP BY site_id, strategy" +
                    ");";
                command.ExecuteNonQuery();
            }
        }

        // Добавляем метод загрузки всех записей из таблицы hosting_list
        public List<HostingRecord> LoadHostingList()
        {
            var result = new List<HostingRecord>();
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, name, ip FROM hosting_list ORDER BY id;";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                            string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            string ip = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            result.Add(new HostingRecord(id, name, ip));
                        }
                    }
                }
            }
            return result;
        }

        // Добавляем метод вставки новой записи в hosting_list с возвратом идентификатора
        public long InsertHostingRecord(string name, string ip)
        {
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var transaction = _connection.BeginTransaction())
                {
                    using (var command = _connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "INSERT INTO hosting_list (name, ip) VALUES ($name, $ip);";
                        command.Parameters.AddWithValue("$name", name ?? string.Empty);
                        command.Parameters.AddWithValue("$ip", ip ?? string.Empty);
                        command.ExecuteNonQuery();
                    }

                    using (var idCommand = _connection.CreateCommand())
                    {
                        idCommand.Transaction = transaction;
                        idCommand.CommandText = "SELECT last_insert_rowid();";
                        long id = Convert.ToInt64(idCommand.ExecuteScalar());
                        transaction.Commit();
                        return id;
                    }
                }
            }
        }

        // Добавляем метод обновления отдельных полей таблицы hosting_list
        public void UpdateHostingField(long id, string fieldName, string value)
        {
            if (fieldName != "name" && fieldName != "ip")
            {
                throw new ArgumentException("Недопустимое имя поля hosting_list", nameof(fieldName));
            }

            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE hosting_list SET {fieldName} = $value WHERE id = $id;";
                    command.Parameters.AddWithValue("$value", value ?? string.Empty);
                    command.Parameters.AddWithValue("$id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Добавляем метод загрузки всех записей из таблицы cpa_list
        public List<CpaRecord> LoadCpaList()
        {
            var result = new List<CpaRecord>();
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, name, login, url, script, description FROM cpa_list ORDER BY id;";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                            string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            string login = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            string url = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                            string script = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                            string description = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            result.Add(new CpaRecord(id, name, login, url, script, description));
                        }
                    }
                }
            }
            return result;
        }

        // Добавляем метод вставки новой записи в cpa_list
        public long InsertCpaRecord(string name, string login, string url, string script, string description)
        {
            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var transaction = _connection.BeginTransaction())
                {
                    using (var command = _connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "INSERT INTO cpa_list (name, login, url, script, description) VALUES ($name, $login, $url, $script, $description);";
                        command.Parameters.AddWithValue("$name", name ?? string.Empty);
                        command.Parameters.AddWithValue("$login", login ?? string.Empty);
                        command.Parameters.AddWithValue("$url", url ?? string.Empty);
                        command.Parameters.AddWithValue("$script", script ?? string.Empty);
                        command.Parameters.AddWithValue("$description", description ?? string.Empty);
                        command.ExecuteNonQuery();
                    }

                    using (var idCommand = _connection.CreateCommand())
                    {
                        idCommand.Transaction = transaction;
                        idCommand.CommandText = "SELECT last_insert_rowid();";
                        long id = Convert.ToInt64(idCommand.ExecuteScalar());
                        transaction.Commit();
                        return id;
                    }
                }
            }
        }

        // Добавляем метод обновления безопасно ограниченных полей таблицы cpa_list
        public void UpdateCpaField(long id, string fieldName, string value)
        {
            if (fieldName != "name" && fieldName != "login" && fieldName != "url" && fieldName != "script" && fieldName != "description")
            {
                throw new ArgumentException("Недопустимое имя поля cpa_list", nameof(fieldName));
            }

            lock (_dbSync)
            {
                EnsureConnectionOpen();
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE cpa_list SET {fieldName} = $value WHERE id = $id;";
                    command.Parameters.AddWithValue("$value", value ?? string.Empty);
                    command.Parameters.AddWithValue("$id", id);
                    command.ExecuteNonQuery();
                }
            }
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

    // Добавляем структуру-обертку для записей hosting_list
    public sealed class HostingRecord
    {
        public HostingRecord(long id, string name, string ip)
        {
            Id = id;
            Name = name;
            Ip = ip;
        }

        public long Id { get; }
        public string Name { get; }
        public string Ip { get; }
    }

    // Добавляем структуру-обертку для доменов, содержащую русские и punycode-наименования
    public sealed class DomainRecord
    {
        public DomainRecord(long id, string name, string rusName, string ip)
        {
            Id = id;
            Name = name ?? string.Empty;
            RusName = rusName ?? string.Empty;
            Ip = ip ?? string.Empty;
        }

        public long Id { get; }
        public string Name { get; }
        public string RusName { get; }
        public string Ip { get; }
    }

    // Добавляем структуру-обертку для сайтов при определении хостинга
    public sealed class SiteHostingCandidate
    {
        public SiteHostingCandidate(long id, string pageAddress, long currentHostingId)
        {
            Id = id;
            PageAddress = pageAddress ?? string.Empty;
            CurrentHostingId = currentHostingId;
        }

        public long Id { get; }
        public string PageAddress { get; }
        public long CurrentHostingId { get; }
    }

    // Добавляем структуру-обертку для запуска парсеров по выбранным сайтам
    public sealed class SiteParserTarget
    {
        public SiteParserTarget(long id, string pageAddress, string query)
        {
            Id = id;
            PageAddress = pageAddress ?? string.Empty;
            Query = query ?? string.Empty;
        }

        public long Id { get; }
        public string PageAddress { get; }
        public string Query { get; }
    }

    // Добавляем структуру-обертку для сайтов при определении CPA
    public sealed class SiteCpaCandidate
    {
        public SiteCpaCandidate(long id, string pageAddress, long currentCpaId)
        {
            Id = id;
            PageAddress = pageAddress ?? string.Empty;
            CurrentCpaId = currentCpaId;
        }

        public long Id { get; }
        public string PageAddress { get; }
        public long CurrentCpaId { get; }
    }

    // Добавляем структуру-обертку для записей cpa_list
    public sealed class CpaRecord
    {
        public CpaRecord(long id, string name, string login, string url, string script, string description)
        {
            Id = id;
            Name = name;
            Login = login;
            Url = url;
            Script = script;
            Description = description;
        }

        public long Id { get; }
        public string Name { get; }
        public string Login { get; }
        public string Url { get; }
        public string Script { get; }
        public string Description { get; }
    }
}
