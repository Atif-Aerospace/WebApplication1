using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using Aircadia.ObjectModel.Treatments.DOE;


using System.Data;
using System.Data.SqlServerCe;
using System.IO;


using System.Windows;


using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Workflows;

namespace Aircadia.ObjectModel.Treatments.MVDA
{
    [Serializable()]
    public class KMeans : MVDA
    {
        // Data
        public double[,] data;// = {{1, 1}, {2, 1}, {4, 3}, {5, 4}};

        // Initial centroids
        public double[,] centroidMatrix;

        // Distance matrix
        public double[,] distanceMatrix;
        // Group category matrix
        public double[,] clusterMatrixInitial;
        public double[,] clusterMatrix;

        public int noOfVariables;
        public int noOfEntries;
		public int NoOfClusters { get; set; }

		readonly List<string> columnNames = new List<string>();
		readonly List<string> columnTypes = new List<string>();
		readonly List<int> clusterCat = new List<int>();


        public string inputDatabasePath = "H:\\Aircadia\\Projects\\AtifSunDOE\\Studies\\sssDOE\\rrr1.sdf";

        public List<string> SelectedNames
        {
            get;
            set;
        }
        public List<string> selectedTypes = new List<string>();

        public List<List<double>> Centroids
        {
            get;
            set;
        }



        public KMeans(string name, string description) :
            base(name, description)
        {
        }


		public override bool ApplyOn(ExecutableComponent ec) => ApplyOn();
		public override bool ApplyOn()
        {
            /*
            this.columnNames.Clear();
            this.columnTypes.Clear();


            this.Result.NoOfBatches = 1;
            this.Result.BatchSize = 300;

            // Initialise data
            this.data = new double[this.noOfEntries, this.noOfVariables];









            string connectionString = string.Format("Data Source = " + inputDatabasePath + ";Persist Security Info=False");
            SqlCeConnection inputDatabaseCon = new SqlCeConnection(connectionString);

            // Initialises centroids for all clusters
            if (inputDatabaseCon.State == ConnectionState.Closed)
                inputDatabaseCon.Open();

            try
            {

                string sql1 = "select * from METADATA";
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








                // selected types
                string sql = "select Name, Type from METADATA";
                cmd = new SqlCeCommand(sql, inputDatabaseCon);
                cmd.CommandType = CommandType.Text;
                SqlCeResultSet rs = cmd.ExecuteResultSet(ResultSetOptions.Scrollable);
                if (rs.HasRows)
                {
                    while (rs.Read())
                    {
                        string colName = rs.GetString(0);
                        foreach (string selectedColName in this.SelectedNames)
                        {
                            if (colName == selectedColName)
                            {
                                this.selectedTypes.Add(rs.GetString(1));
                                break;
                            }
                        }
                    }
                }


                sql = "select ";
                for (int i = 0; i < this.SelectedNames.Count; i++)
                {
                    sql += this.SelectedNames[i] + ", ";
                }
                sql = sql.TrimEnd(',', ' ');
                sql += " from DataAggregation";
                string tabName = this.Result.TableNames[0];
                cmd = new SqlCeCommand(sql, inputDatabaseCon);
                cmd.CommandType = CommandType.Text;

                rs = cmd.ExecuteResultSet(ResultSetOptions.Scrollable);
                if (rs.HasRows)
                {
                    int entryID = 0;
                    while (rs.Read())
                    {
                        for (int k = 0; k < this.noOfVariables; k++)
                        {
                            if (this.selectedTypes[k] == "INTEGER")
                                this.data[entryID, k] = rs.GetInt32(k);
                            else if (this.selectedTypes[k] == "DOUBLE")
                                this.data[entryID, k] = rs.GetDouble(k);
                        }
                        entryID++;
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












            bool status = true;
            // Assign the centroid matrix (initial guess is first noOfCluster entries)

            // Initialise the centroids
            this.centroidMatrix = new double[noOfClusters, this.noOfVariables];
            for (int i = 0; i < this.noOfClusters; i++)
            {
                for (int j = 0; j < this.noOfVariables; j++)
                {
                    this.centroidMatrix[i, j] = this.Centroids[i][j];
                }
            }
            


            // Initialise distance matrix
            this.distanceMatrix = new double[noOfClusters, this.noOfEntries];
            // Initialise group matrix
            this.clusterMatrixInitial = new double[noOfClusters, this.noOfEntries];
            this.clusterMatrix = new double[noOfClusters, this.noOfEntries];







            ComputeDistanceMatrix();
            ComputeClusterMatrix();
            
            while (true)
            {
                ComputeCentroid();
                ComputeDistanceMatrix();
                ComputeClusterMatrix();
                if (CompareClusterMatrix())
                    break;

                // Set current cluster matrix to previous cluster matrix
                for (int i = 0; i < this.noOfClusters; i++)
                    for (int j = 0; j < this.noOfEntries; j++)
                        this.clusterMatrixInitial[i, j] = this.clusterMatrix[i, j];
                break;
            }
            

            for (int i = 0; i < this.noOfEntries; i++)
            {
                for (int j = 0; j < this.noOfClusters; j++)
                {
                    if (this.clusterMatrix[j, i] == 1)
                        clusterCat.Add(j);
                }
            }



















            
            #region Meta-Data Extraction
            connectionString = string.Format("Data Source = " + this.inputDatabasePath + ";Persist Security Info=False");
            inputDatabaseCon = new SqlCeConnection(connectionString);
            if (inputDatabaseCon.State == ConnectionState.Closed)
                inputDatabaseCon.Open();

            string sql11 = "select Name, Type from METADATA";

            try
            {
                SqlCeCommand cmd = new SqlCeCommand(sql11, inputDatabaseCon);
                cmd.CommandType = CommandType.Text;

                SqlCeDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    columnNames.Add(dataReader.GetString(0));
                    columnTypes.Add(dataReader.GetString(1));
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
            















            #region Create Output Database (Results Database)
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



            // SQL create statement
            this.Result.TableNames.Add("Cluster");
            string sqlCreate = "create table Cluster (";

            // SQL insert statement
            string sqlInsert = "insert into Cluster ( ";
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
            sqlCreate += "ClusterCat" + " INT)";
            sqlInsert += "ClusterCat" + ")";
            sqlInsertValues += "@" + "ClusterCat" + ")";

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
























            if (inputDatabaseCon.State == ConnectionState.Closed)
                inputDatabaseCon.Open();

            try
            {
                string sql = "select * from " + "DataAggregation";

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
                        // Clustering category column
                        insertSqlCeCommand.Parameters.AddWithValue("@ClusterCat", clusterCat[ccccc++]);
                        insertSqlCeCommand.ExecuteNonQuery();
                        insertSqlCeCommand.Parameters.Clear();
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










            connection.Close();
            */
            bool status = true;
            return status;
        }

        public void ComputeCentroid()
        {
            // Clear and Copy centroid matrix
            double[,] copyCentroidMatrix = new double[NoOfClusters, noOfVariables];
            for (int i = 0; i < NoOfClusters; i++)
            {
                for (int j = 0; j < noOfVariables; j++)
                {
                    copyCentroidMatrix[i, j] = centroidMatrix[i, j];
					centroidMatrix[i, j] = 0;
                }
            }

            for (int i = 0; i < NoOfClusters; i++)
            {
                int noOfClusterEntries = 0;
                for (int j = 0; j < noOfEntries; j++)
                {
                    if (clusterMatrix[i, j] == 1)
                    {
                        noOfClusterEntries++;
                        for (int k = 0; k < noOfVariables; k++)
                        {
							centroidMatrix[i, k] += data[j, k];
                        }
                    }
                }
                if (noOfClusterEntries != 0)
                {
                    for (int j = 0; j < noOfVariables; j++)
						centroidMatrix[i, j] /= noOfClusterEntries;
                }
                else
                {
                    for (int j = 0; j < noOfVariables; j++)
						centroidMatrix[i, j] = copyCentroidMatrix[i, j];
                }
            }
        }

        public void ComputeDistanceMatrix()
        {
            for (int i = 0; i < noOfEntries; i++)
            {
                for (int j = 0; j < NoOfClusters; j++)
                {
                    double distance = 0;
                    for (int k = 0; k < noOfVariables; k++)
                    {
                        distance += Math.Pow(data[i, k] - centroidMatrix[j, k], 2);
                    }
					distanceMatrix[j, i] = Math.Sqrt(distance);
                }
            }
        }

        public void ComputeClusterMatrix()
        {
            // Clear cluster matrix
            for (int i = 0; i < NoOfClusters; i++)
                for (int j = 0; j < noOfEntries; j++)
					clusterMatrix[i, j] = 0;

            for (int i = 0; i < noOfEntries; i++)
            {
                int clusterID = 0;
                double minDistance = distanceMatrix[0, i];
                for (int j = 1; j < NoOfClusters; j++)
                {
                    if (minDistance > distanceMatrix[j, i])
                    {
                        clusterID = j;
                        minDistance = distanceMatrix[j, i];
                    }
                }

				// Assign to cluster matrix
				clusterMatrix[clusterID, i] = 1;
            }
        }

        public bool CompareClusterMatrix()
        {
            bool sameClusterMatrix = true;

            for (int i = 0; i < NoOfClusters; i++)
            {
                if (!sameClusterMatrix)
                    break;

                for (int j = 0; j < noOfEntries; j++)
                {
                    if (clusterMatrixInitial[i, j] != clusterMatrix[i, j])
                    {
                        sameClusterMatrix = false;
                        break;
                    }
                }
            }

            return sameClusterMatrix;
        }
    }
}
