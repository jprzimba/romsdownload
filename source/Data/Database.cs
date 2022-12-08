using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;

namespace romsdownload.Data
{
    public class Database
    {
        private static SQLiteConnection sqliteConnection;

        public Database() { }

        public static SQLiteConnection Connection()
        {
            sqliteConnection = new SQLiteConnection("Data Source=" + Directories.DatabasePath + "; Version = 3;");
            try
            {
                sqliteConnection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return sqliteConnection;
        }

        public static void CreateDatabase()
        {
            try
            {
                SQLiteConnection.CreateFile(Directories.DatabasePath);
                CreateTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void CreateTables()
        {
            try
            {
                //Games Table
                SQLiteCommand cmd = Connection().CreateCommand();
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Games(id INTEGER PRIMARY KEY AUTOINCREMENT, title TEXT NOT NULL COLLATE NOCASE, image TEXT NOT NULL, url TEXT NOT NULL, plataform TEXT NOT NULL, UNIQUE(url));";
                cmd.ExecuteNonQuery();

                //Downloads Table
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Downloads(id INTEGER PRIMARY KEY AUTOINCREMENT, memorycachesize INTEGER NOT NULL, maxdownloads INTEGER NOT NULL, enablespeedlimit INTEGER NOT NULL, speedlimit INTEGER NOT NULL, startdownloadsonstartup INTEGER NOT NULL, startimmediately INTEGER NOT NULL, downloadpath TEXT NOT NULL);";
                cmd.ExecuteNonQuery();

                //Theme Table
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Theme(style TEXT NOT NULL DEFAULT Light, color TEXT NOT NULL DEFAULT Blue);";
                cmd.ExecuteNonQuery();

                //Insert values
                //Theme
                cmd.CommandText = "INSERT INTO Theme(style, color) VALUES (@style, @color)";
                cmd.Parameters.AddWithValue("@style", "Light");
                cmd.Parameters.AddWithValue("@color", "Blue");
                cmd.ExecuteNonQuery();

                //Downloads
                cmd.CommandText = "INSERT INTO Downloads(memorycachesize, maxdownloads, enablespeedlimit, speedlimit, startdownloadsonstartup, startimmediately, downloadpath) VALUES (@memorycachesize, @maxdownloads, @enablespeedlimit, @speedlimit, @startdownloadsonstartup, @startimmediately, @downloadpath)";
                cmd.Parameters.AddWithValue("@memorycachesize", 1024);
                cmd.Parameters.AddWithValue("@maxdownloads", 5);
                cmd.Parameters.AddWithValue("@enablespeedlimit", 0);
                cmd.Parameters.AddWithValue("@speedlimit", 200);
                cmd.Parameters.AddWithValue("@startdownloadsonstartup", 1);
                cmd.Parameters.AddWithValue("@startimmediately", 1);
                cmd.Parameters.AddWithValue("@downloadpath", "");
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (Connection().State == ConnectionState.Open)
                    Connection().Close();
            }
        }

        public static bool ColumnPlataformHasValue(string tableName, string text)
        {
            try
            {
                using (var cmd = new SQLiteCommand(Connection()))
                {
                    cmd.CommandText = "SELECT count(*) FROM '" + tableName + "' Where plataform='" + text + "'";
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count > 0)
                    {
                        //MessageBox.Show(count.ToString());
                        return true;
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (Connection().State == ConnectionState.Open)
                    Connection().Close();
            }

            return false;
        }

        public static void Add(string title, string image, string url, string plataform)
        {
            try
            {
                using (var cmd = Connection().CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Games(title, image, url, plataform) VALUES (@title, @image, @url, @plataform)";
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@image", image);
                    cmd.Parameters.AddWithValue("@url", url);
                    cmd.Parameters.AddWithValue("@plataform", plataform);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {

            }
            finally
            {
                if (Connection().State == ConnectionState.Open)
                    Connection().Close();
            }
        }

        public static void UpdateTheme(string style, string color)
        {
            try
            {
                using (var cmd = new SQLiteCommand(Connection()))
                {
                    cmd.CommandText = "UPDATE Theme SET style=@style, color=@color";
                    cmd.Parameters.AddWithValue("@style", style);
                    cmd.Parameters.AddWithValue("@color", color);
                    cmd.ExecuteNonQuery();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (Connection().State == ConnectionState.Open)
                    Connection().Close();
            }
        }

        public static void SaveDownloadPath(string path)
        {
            try
            {
                using (var cmd = new SQLiteCommand(Connection()))
                {
                    cmd.CommandText = "UPDATE Downloads SET downloadpath=@downloadpath";
                    cmd.Parameters.AddWithValue("@downloadpath", path);
                    cmd.ExecuteNonQuery();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (Connection().State == ConnectionState.Open)
                    Connection().Close();
            }
        }

        public static void Delete(int id)
        {
            try
            {
                using (var cmd = new SQLiteCommand(Connection()))
                {
                    cmd.CommandText = "DELETE FROM Games Where id=@id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (Connection().State == ConnectionState.Open)
                    Connection().Close();
            }
        }

        public static DataSet GetGameById(int id)
        {
            SQLiteDataAdapter da = null;
            DataSet dt = new DataSet();
            try
            {
                using (var cmd = Connection().CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Games Where id=" + id;
                    da = new SQLiteDataAdapter(cmd.CommandText, Connection());
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (Connection().State == ConnectionState.Open)
                    Connection().Close();
            }

            return dt;
        }

        public static DataSet GetGamesByName(string name)
        {
            SQLiteDataAdapter da = null;
            DataSet dt = new DataSet();
            try
            {
                using (var cmd = Connection().CreateCommand())
                {
                    cmd.CommandText = "SELECT title FROM Games Where title LIKE ('%"+name+"%')";
                    da = new SQLiteDataAdapter(cmd.CommandText, Connection());
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (Connection().State == ConnectionState.Open)
                    Connection().Close();
            }

            return dt;
        }

        public static DataSet GetGamesByNameAndPlataform(string name, string plataform)
        {
            SQLiteDataAdapter da = null;
            DataSet dt = new DataSet();
            try
            {
                using (var cmd = Connection().CreateCommand())
                {
                    cmd.CommandText = "SELECT title FROM Games Where title LIKE ('%" + name + "%') AND plataform='" + plataform + "'";
                    da = new SQLiteDataAdapter(cmd.CommandText, Connection());
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (Connection().State == ConnectionState.Open)
                    Connection().Close();
            }

            return dt;
        }

        public static DataSet GetGamesByPlataform(string plataform)
        {
            SQLiteDataAdapter da = null;
            DataSet ds = new DataSet();
            try
            {
                using (var cmd = Connection().CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Games WHERE plataform='" + plataform + "'";
                    da = new SQLiteDataAdapter(cmd.CommandText, Connection());
                    da.Fill(ds);
                    return ds;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (Connection().State == ConnectionState.Open)
                    Connection().Close();
            }

            return ds;
        }
    }
}