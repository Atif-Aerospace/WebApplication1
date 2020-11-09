using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using System.Data;
using System.Data.SqlServerCe;
using System.IO;

using System.Windows;

using Aircadia.ObjectModel.DataObjects;


namespace Aircadia.ObjectModel.Treatments.MVDA
{
    [Serializable()]
    public class DataAggregation : MVDA
    {
		readonly List<string> columnNames = new List<string>();
		readonly List<string> columnTypes = new List<string>();

        public List<string> Databases
        {
            get;
            set;
        }


        public DataAggregation(string name, string description) :
            base(name, description)
        {
        }


		public override bool ApplyOn(ExecutableComponent ec) => true;
		public override bool ApplyOn()
        {
            bool status = true;
            /*



            this.Result.NoOfBatches = 1;
            this.Result.BatchSize = 300;
            this.Result.TableNames.Add("DoE");


            


            #region Meta-Data Extraction
            string connectionString = string.Format("Data Source = " + this.Databases[0] + ";Persist Security Info=False");
            SqlCeConnection inputDatabaseCon = new SqlCeConnection(connectionString);
            if (inputDatabaseCon.State == ConnectionState.Closed)
                inputDatabaseCon.Open();

            string sql1 = "select * from METADATA";

            try
            {
                SqlCeCommand cmd = new SqlCeCommand(sql1, inputDatabaseCon);
                cmd.CommandType = CommandType.Text;

                SqlCeDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    string columnName = dataReader.GetString(0);
                    columnNames.Add(columnName);
                    this.Result.ColumnNames.Add(columnName);
                    string columnType = dataReader.GetString(1);
                    columnTypes.Add(columnType);
                    if (columnType == "INTEGER")
                    {
                        this.Result.ColumnTypes.Add(DataTypes.INTEGER);
                        this.Result.ColumnFormats.Add(0);
                    }
                    else if (columnType == "DOUBLE")
                    {
                        this.Result.ColumnTypes.Add(DataTypes.DOUBLE);
                        this.Result.ColumnFormats.Add(4);
                    }
                    string columnUnit = dataReader.GetString(2);
                    this.Result.ColumnUnits.Add(columnUnit);
                    string minValue = dataReader.GetString(3);
                    if (minValue == "")
                        this.Result.MinValues.Add(Double.NaN);
                    else
                        this.Result.MinValues.Add(Convert.ToDouble(minValue));
                    string maxValue = dataReader.GetString(4);
                    if (maxValue == "")
                        this.Result.MaxValues.Add(Double.NaN);
                    else
                        this.Result.MaxValues.Add(Convert.ToDouble(maxValue));
                }
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message, "Oh Crap.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Oh Crap.");
            }
            finally
            {
                // Don’t need it anymore so we’ll be good and close it.
                // in a ‘real life’ situation
                // cn would likely be class level
                inputDatabaseCon.Close();
            }
            #endregion Meta-Data Extraction


            foreach (string sss in this.Databases)
            {
                connectionString = string.Format("Data Source = " + sss + ";Persist Security Info=False");
                SqlCeConnection inputDatabaseCon1 = new SqlCeConnection(connectionString);
                if (inputDatabaseCon1.State == ConnectionState.Closed)
                    inputDatabaseCon1.Open();

                sql1 = "select MinValue, MaxValue from METADATA";

                try
                {
                    SqlCeCommand cmd = new SqlCeCommand(sql1, inputDatabaseCon1);
                    cmd.CommandType = CommandType.Text;

                    SqlCeDataReader dataReader = cmd.ExecuteReader();
                    int cont = 0;
                    while (dataReader.Read())
                    {
                        string minValue = dataReader.GetString(0);
                        if (minValue != "" && Convert.ToDouble(minValue) < this.Result.MinValues[cont])
                            this.Result.MinValues[cont] = Convert.ToDouble(minValue);
                        string maxValue = dataReader.GetString(1);
                        if (maxValue != "" && Convert.ToDouble(maxValue) > this.Result.MaxValues[cont])
                            this.Result.MaxValues[cont] = Convert.ToDouble(maxValue);
                        cont++;
                    }
                }
                catch (SqlCeException sqlexception)
                {
                    MessageBox.Show(sqlexception.Message, "Oh Crap.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Oh Crap.");
                }
                finally
                {
                    // Don’t need it anymore so we’ll be good and close it.
                    // in a ‘real life’ situation
                    // cn would likely be class level
                    inputDatabaseCon1.Close();
                }
            }











            // Meta-Data
            // Create Database and store results
            string databaseFileName = Path.Combine(AircadiaProject.ProjectPath, this.Result.DatabasePath); // Microsoft SQL server compact edition file

            // Create database for optimisation results
            if (File.Exists(databaseFileName))
            {
                File.Delete(databaseFileName);
            }
            connectionString = string.Format("Data Source = " + databaseFileName + ";Persist Security Info=False");
            SqlCeEngine engine = new SqlCeEngine(connectionString);
            engine.CreateDatabase();

            // Create tables
            SqlCeConnection connection = new SqlCeConnection(connectionString);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            string sqlCreateMetaData = "create table METADATA (Name NTEXT, Type NTEXT, Unit NTEXT, MinValue NTEXT, MaxValue NTEXT)";
            string sqlInsertMetaData = "insert into METADATA (Name, Type, Unit, MinValue, MaxValue) values (@Name, @Type, @Unit, @MinValue, @MaxValue)";
            SqlCeCommand createMetaDataSqlCeCommand = new SqlCeCommand(sqlCreateMetaData, connection);
            SqlCeCommand insertMetaDataSqlCeCommand = new SqlCeCommand(sqlInsertMetaData, connection);
            try
            {
                createMetaDataSqlCeCommand.ExecuteNonQuery();
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message, "Oh Crap.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Oh Crap.");
            }




            for (int i = 0; i < this.Result.ColumnNames.Count; i++)
            {
                insertMetaDataSqlCeCommand.Parameters.Clear();
                insertMetaDataSqlCeCommand.Parameters.AddWithValue("@Name", this.Result.ColumnNames[i]);
                if (this.Result.ColumnTypes[i] == DataTypes.INTEGER)
                    insertMetaDataSqlCeCommand.Parameters.AddWithValue("@Type", "INTEGER");
                else if (this.Result.ColumnTypes[i] == DataTypes.DOUBLE)
                    insertMetaDataSqlCeCommand.Parameters.AddWithValue("@Type", "DOUBLE");
                insertMetaDataSqlCeCommand.Parameters.AddWithValue("@Unit", this.Result.ColumnUnits[i]);
                insertMetaDataSqlCeCommand.Parameters.AddWithValue("@MinValue", this.Result.MinValues[i].ToString());
                insertMetaDataSqlCeCommand.Parameters.AddWithValue("@MaxValue", this.Result.MaxValues[i].ToString());
                insertMetaDataSqlCeCommand.ExecuteNonQuery();
            }



















            #region Create Output Database (Results Database)
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }



            // SQL create statement
            this.Result.TableNames.Add("DataAggregation");
            string sqlCreate = "create table DataAggregation (";

            // SQL insert statement
            string sqlInsert = "insert into DataAggregation ( ";
            string sqlInsertValues = "values ( ";

            // Factors
            for (int i = 0; i < columnNames.Count; i++)
            {

                if (columnTypes[i] == "INTEGER")
                {
                    sqlCreate += columnNames[i] + " int, ";
                    sqlInsert += columnNames[i] + ", ";
                    sqlInsertValues += "@" + columnNames[i] + ", ";
                }
                else if (columnTypes[i] == "DOUBLE")
                {
                    sqlCreate += columnNames[i] + " float, ";
                    sqlInsert += columnNames[i] + ", ";
                    sqlInsertValues += "@" + columnNames[i] + ", ";
                }
                else if (columnTypes[i] == "STRING")
                {
                    sqlCreate += columnNames[i] + " NTEXT, ";
                    sqlInsert += columnNames[i] + ", ";
                    sqlInsertValues += "@" + columnNames[i] + ", ";
                }
            }
            sqlCreate = sqlCreate.Remove(sqlCreate.Length - 2);
            sqlInsert = sqlInsert.Remove(sqlInsert.Length - 2);
            sqlInsertValues = sqlInsertValues.Remove(sqlInsertValues.Length - 2);

            sqlCreate += ")";
            sqlInsert += ")";
            sqlInsertValues += ")";
            sqlInsert += (" " + sqlInsertValues);

            // Create SqlCe commands
            SqlCeCommand createSqlCeCommand = new SqlCeCommand(sqlCreate, connection);
            SqlCeCommand insertSqlCeCommand = new SqlCeCommand(sqlInsert, connection);


            // Execute the SQL command
            try
            {
                createSqlCeCommand.ExecuteNonQuery();
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message, "Oh Crap.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Oh Crap.");
            }
            
            #endregion Create Output Database (Results Database)






















            int counter = 1;
            foreach (string sss in this.Databases)
            {
                string connectionString1 = string.Format("Data Source = " + sss + ";Persist Security Info=False");
                inputDatabaseCon = new SqlCeConnection(connectionString1);

                if (inputDatabaseCon.State == ConnectionState.Closed)
                    inputDatabaseCon.Open();

                try
                {
                    string sql = "select * from " + "DoE";

                    SqlCeCommand cmd = new SqlCeCommand(sql, inputDatabaseCon);
                    cmd.CommandType = CommandType.Text;

                    SqlCeResultSet rs = cmd.ExecuteResultSet(ResultSetOptions.Scrollable);
                    if (rs.HasRows)
                    {
                        int ccccc = 0;
                        while (rs.Read())
                        {
                            for (int i = 0; i < columnNames.Count; i++)
                            {
                                if (columnNames[i] == "ID")
                                {
                                    insertSqlCeCommand.Parameters.AddWithValue("@ID", counter);
                                }
                                else
                                {
                                    if (columnTypes[i] == "INTEGER")
                                    {
                                        insertSqlCeCommand.Parameters.AddWithValue("@" + columnNames[i], rs.GetInt32(i));
                                    }
                                    else if (columnTypes[i] == "DOUBLE")
                                    {
                                        insertSqlCeCommand.Parameters.AddWithValue("@" + columnNames[i], rs.GetDouble(i));
                                    }
                                    else if (columnTypes[i] == "STRING")
                                    {
                                        insertSqlCeCommand.Parameters.AddWithValue("@" + columnNames[i], rs.GetString(i));
                                    }
                                }
                            }
                            insertSqlCeCommand.ExecuteNonQuery();
                            insertSqlCeCommand.Parameters.Clear();
                            counter++;
                        }
                    }
                }
                catch (SqlCeException sqlexception)
                {
                    MessageBox.Show(sqlexception.Message, "Oh Crap.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Oh Crap.");
                }
                finally
                {
                    inputDatabaseCon.Close();
                }

            }










            connection.Close();
            */
            return status;
        }
    }
}
