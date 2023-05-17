using Npgsql;

namespace GtkTest
{
    class PostgreSQl
    {
        public NpgsqlDataSource? DataSource { get; set; }

        #region InformationShema

        public bool IfExistsTable(string tableName)
        {
            if (DataSource != null)
            {
                string query = "SELECT table_name " +
                               "FROM information_schema.tables " +
                               "WHERE table_schema = 'public' AND table_type = 'BASE TABLE' AND table_name = @table_name";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("table_name", tableName);

                NpgsqlDataReader reader = command.ExecuteReader();
                bool hasRows = reader.HasRows;

                reader.Close();

                return hasRows;
            }
            else
                return false;
        }

        public bool IfExistsColumn(string tableName, string columnName)
        {
            if (DataSource != null)
            {
                string query = "SELECT column_name " +
                               "FROM information_schema.columns " +
                               "WHERE table_schema = 'public' AND table_name = @table_name AND column_name = @column_name";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("table_name", tableName);
                command.Parameters.AddWithValue("column_name", columnName);

                NpgsqlDataReader reader = command.ExecuteReader();
                bool hasRows = reader.HasRows;

                reader.Close();

                return hasRows;
            }
            else
                return false;
        }

        public async Task<ConfigurationInformationSchema> SelectInformationSchema(CancellationToken token)
        {
            ConfigurationInformationSchema informationSchema = new ConfigurationInformationSchema();

            if (DataSource != null)
            {
                //
                // Таблиці та стовпчики
                //

                string query = "SELECT table_name, column_name, data_type, udt_name " +
                               "FROM information_schema.columns " +
                               "WHERE table_schema = 'public'";

                NpgsqlCommand command = DataSource.CreateCommand(query);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync(token);
                while (await reader.ReadAsync(token))
                {
                    informationSchema.Append(
                        reader["table_name"].ToString()?.ToLower() ?? "",
                        reader["column_name"].ToString()?.ToLower() ?? "",
                        reader["data_type"].ToString() ?? "",
                        reader["udt_name"].ToString() ?? "");
                }
                await reader.CloseAsync();

                //
                // Індекси
                //

                query = "SELECT tablename, indexname FROM pg_indexes WHERE schemaname = 'public'";

                command = DataSource.CreateCommand(query);
                reader = await command.ExecuteReaderAsync(token);
                while (await reader.ReadAsync(token))
                {
                    informationSchema.AppendIndex(
                        reader["tablename"].ToString()?.ToLower() ?? "",
                        reader["indexname"].ToString()?.ToLower() ?? "");
                }
                await reader.CloseAsync();
            }

            return informationSchema;
        }

        #endregion

    }
}