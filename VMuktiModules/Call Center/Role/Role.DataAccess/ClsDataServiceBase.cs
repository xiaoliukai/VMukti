using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Role.Common;
//using VMuktiAPI;

namespace Role.DataAccess
{

    ////////////////////////////////////////////////////////////////////////////
    /// <summary>
    ///   Defines common DataService routines for transaction management, 
    ///   stored procedure execution, parameter creation, and null value 
    ///   checking
    /// </summary>	
    ////////////////////////////////////////////////////////////////////////////
    public class ClsDataServiceBase
    {

        ////////////////////////////////////////////////////////////////////////
        // Fields
        ////////////////////////////////////////////////////////////////////////
        private bool _isOwner = false;   //True if service owns the transaction        
        private SqlTransaction _txn;     //Reference to the current transaction


        ////////////////////////////////////////////////////////////////////////
        // Properties 
        ////////////////////////////////////////////////////////////////////////
        public IDbTransaction Txn
        {
            get { return (IDbTransaction)_txn; }
            set { _txn = (SqlTransaction)value; }
        }


        ////////////////////////////////////////////////////////////////////////
        // Constructors
        ////////////////////////////////////////////////////////////////////////

        public ClsDataServiceBase() : this(null) { }


        public ClsDataServiceBase(IDbTransaction txn)
        {
            if (txn == null)
            {
                _isOwner = true;
            }
            else
            {
                _txn = (SqlTransaction)txn;
                _isOwner = false;
            }
        }


        ////////////////////////////////////////////////////////////////////////
        // Connection and Transaction Methods
        ////////////////////////////////////////////////////////////////////////
        protected static string GetConnectionString()
        {
            try
            {
                return VMuktiAPI.VMuktiInfo.MainConnectionString;
            }
            catch (Exception ex)
            {
                VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "GetConnectionString", "ClsDataServiceBase.cs"); 
                return null;
            }
        }


        public static IDbTransaction BeginTransaction()
        {try
            {
            SqlConnection txnConnection = new SqlConnection(GetConnectionString());
            txnConnection.Open();
            return txnConnection.BeginTransaction();   
             }
            catch (Exception ex)
            {
                VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "BeginTransaction", "ClsDataServiceBase.cs"); 
                return null;
            }
        }


        ////////////////////////////////////////////////////////////////////////
        // ExecuteDataSet Methods
        ////////////////////////////////////////////////////////////////////////
        protected DataSet ExecuteDataSet(string cmdText, CommandType cmdType, params IDataParameter[] procParams)
        {try
            {
            SqlCommand cmd;
            return ExecuteDataSet(out cmd, cmdText, cmdType, procParams);
 }
            catch (Exception ex)
            {
                VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "ExecuteDataSet", "ClsDataServiceBase.cs"); 
                return null;
            }
        }


        protected DataSet ExecuteDataSet(out SqlCommand cmd, string cmdText, CommandType cmdType, params IDataParameter[] procParams)
        {try
            {
            SqlConnection cnx = null;
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            cmd = null;

            try
            {
                //Setup command object
                cmd = new SqlCommand(cmdText);
                cmd.CommandType = cmdType;
                if (procParams != null)
                {
                    for (int index = 0; index < procParams.Length; index++)
                    {
                        cmd.Parameters.Add(procParams[index]);
                    }
                }                
                da.SelectCommand = (SqlCommand)cmd;

                //Determine the transaction owner and process accordingly
                if (_isOwner)
                {
                    cnx = new SqlConnection(GetConnectionString());
                    cmd.Connection = cnx;
                    cnx.Open();
                }
                else
                {
                    cmd.Connection = _txn.Connection;
                    cmd.Transaction = _txn;
                }

                //Fill the dataset
                da.Fill(ds);
            }
            catch
            {
                throw;
            }
            finally
            {
                if(da!=null) da.Dispose();
                if(cmd!=null) cmd.Dispose();
                if (_isOwner)
                {
                    cnx.Dispose(); //Implicitly calls cnx.Close()
                }                
            }
            return ds;
 }
            catch (Exception ex)
            {
                VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "ExecuteDataSet", "ClsDataServiceBase.cs"); 
                cmd = null;
                return null;
            }
        }


        ////////////////////////////////////////////////////////////////////////
        // ExecuteNonQuery Methods
        ////////////////////////////////////////////////////////////////////////
        protected void ExecuteNonQuery(string procName, params IDataParameter[] procParams)
        {try
            {
            SqlCommand cmd;
            ExecuteNonQuery(out cmd, procName, procParams);
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "ExecuteNonQuery", "ClsDataServiceBase.cs"); 
            }
        }


        protected void ExecuteNonQuery(out SqlCommand cmd, string procName, params IDataParameter[] procParams)
        {try
            {
            //Method variables
            SqlConnection cnx = null;
            cmd = null;  //Avoids "Use of unassigned variable" compiler error

            try
            {
                //Setup command object
                cmd = new SqlCommand(procName);
                cmd.CommandType = CommandType.StoredProcedure;
                for (int index = 0; index < procParams.Length; index++)
                {
                    cmd.Parameters.Add(procParams[index]);
                }

                //Determine the transaction owner and process accordingly
                if (_isOwner)
                {
                    cnx = new SqlConnection(GetConnectionString());
                    cmd.Connection = cnx;
                    cnx.Open();
                }
                else
                {
                    cmd.Connection = _txn.Connection;
                    cmd.Transaction = _txn;
                }

                //Execute the command
                cmd.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (_isOwner)
                {
                    cnx.Dispose(); //Implicitly calls cnx.Close()
                }
                if (cmd != null) cmd.Dispose();
            }
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "ExecuteNonQuery", "ClsDataServiceBase.cs"); 
                cmd = null;
            }
        }


        ////////////////////////////////////////////////////////////////////////
        // CreateParameter Methods
        ////////////////////////////////////////////////////////////////////////
        protected SqlParameter CreateParameter(string paramName, SqlDbType paramType, object paramValue)
        {try
            {
            SqlParameter param = new SqlParameter(paramName, paramType);
            
            if (paramValue != DBNull.Value)
            {
                switch (paramType)
                {
                    case SqlDbType.VarChar:
                    case SqlDbType.NVarChar:
                    case SqlDbType.Char:
                    case SqlDbType.NChar:
                    case SqlDbType.Text:
                        paramValue = CheckParamValue((string)paramValue);
                        break;
                    case SqlDbType.DateTime:
                        paramValue = CheckParamValue((DateTime)paramValue);
                        break;
                    case SqlDbType.Int:
                        paramValue = CheckParamValue((int)paramValue);
                        break;
                    case SqlDbType.UniqueIdentifier:
                        paramValue = CheckParamValue(GetGuid(paramValue));
                        break;
                    case SqlDbType.Bit:
                        if (paramValue is bool) paramValue = (int)((bool)paramValue ? 1 : 0);
                        if ((int)paramValue < 0 || (int)paramValue > 1) paramValue = ClsConstants.NullInt;
                        paramValue = CheckParamValue((int)paramValue);
                        break;
                    case SqlDbType.Float:
                        paramValue = CheckParamValue(Convert.ToSingle(paramValue));
                        break;
                    case SqlDbType.Decimal:
                        paramValue = CheckParamValue((decimal)paramValue);
                        break;
                }
            }
            param.Value = paramValue;
            return param;
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CreateParameter", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected SqlParameter CreateParameter(string paramName, SqlDbType paramType, ParameterDirection direction)
        {try
            {
            SqlParameter returnVal = CreateParameter(paramName, paramType, DBNull.Value);
            returnVal.Direction = direction;
            return returnVal;
            }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CreateParameter", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected SqlParameter CreateParameter(string paramName, SqlDbType paramType, object paramValue, ParameterDirection direction)
        {try
            {
            SqlParameter returnVal = CreateParameter(paramName, paramType, paramValue);
            returnVal.Direction = direction;
            return returnVal;
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CreateParameter", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected SqlParameter CreateParameter(string paramName, SqlDbType paramType, object paramValue, int size)
        {try
            {
            SqlParameter returnVal = CreateParameter(paramName, paramType, paramValue);
            returnVal.Size = size;
            return returnVal;
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CreateParameter", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected SqlParameter CreateParameter(string paramName, SqlDbType paramType, object paramValue, int size, ParameterDirection direction)
        {try
            {
            SqlParameter returnVal = CreateParameter(paramName, paramType, paramValue);
            returnVal.Direction = direction;
            returnVal.Size = size;
            return returnVal;
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CreateParameter", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected SqlParameter CreateParameter(string paramName, SqlDbType paramType, object paramValue, int size, byte precision)
        {try
            {
            SqlParameter returnVal = CreateParameter(paramName, paramType, paramValue);
            returnVal.Size = size;
            ((SqlParameter)returnVal).Precision = precision;
            return returnVal;
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CreateParameter", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected SqlParameter CreateParameter(string paramName, SqlDbType paramType, object paramValue, int size, byte precision, ParameterDirection direction)
        {try
            {
            SqlParameter returnVal = CreateParameter(paramName, paramType, paramValue);
            returnVal.Direction = direction;
            returnVal.Size = size;
            returnVal.Precision = precision;
            return returnVal;
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CreateParameter", "ClsDataServiceBase.cs"); 
                return null;
            }
        }


        ////////////////////////////////////////////////////////////////////////
        // CheckParamValue Methods
        ////////////////////////////////////////////////////////////////////////
        protected Guid GetGuid(object value)
        {try
            {
            Guid returnVal = ClsConstants.NullGuid;
            if (value is string)
            {
                returnVal = new Guid((string)value);
            }
            else if (value is Guid)
            {
                returnVal = (Guid)value;
            }
            return returnVal;
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "GetGuid", "ClsDataServiceBase.cs"); 
                Guid a = new Guid();
                return a;
            }
        }

        protected object CheckParamValue(string paramValue)
        {
            try
            {
            if (string.IsNullOrEmpty(paramValue))
            {
                return DBNull.Value;
            }
            else
            {
                return paramValue;
            }
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CheckParamValue", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected object CheckParamValue(Guid paramValue)
        {try
            {
            if (paramValue.Equals(ClsConstants.NullGuid))
            {
                return DBNull.Value;
            }
            else
            {
                return paramValue;
            }
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CheckParamValue", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected object CheckParamValue(DateTime paramValue)
        {try
            {
            if (paramValue.Equals(ClsConstants.NullDateTime))
            {
                return DBNull.Value;
            }
            else
            {
                return paramValue;
            }
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CheckParamValue", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected object CheckParamValue(double paramValue)
        {try
            {
            if (paramValue.Equals(ClsConstants.NullDouble))
            {
                return DBNull.Value;
            }
            else
            {
                return paramValue;
            }
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CheckParamValue", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected object CheckParamValue(float paramValue)
        {try
            {
            if (paramValue.Equals(ClsConstants.NullFloat))
            {
                return DBNull.Value;
            }
            else
            {
                return paramValue;
            }
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CheckParamValue", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected object CheckParamValue(Decimal paramValue)
        {try
            {
            if (paramValue.Equals(ClsConstants.NullDecimal))
            {
                return DBNull.Value;
            }
            else
            {
                return paramValue;
            }
 }
            catch (Exception ex)
            {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CheckParamValue", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

        protected object CheckParamValue(int paramValue)
        {
            try
            {
            if (paramValue.Equals(ClsConstants.NullInt))
            {
                return DBNull.Value;                
            }
            else
            {
                return paramValue;
            }
 }
            catch (Exception ex)
            {
                VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "CheckParamValue", "ClsDataServiceBase.cs"); 
                return null;
            }
        }

    } //class 

} //namespace