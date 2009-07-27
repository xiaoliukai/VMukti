﻿/* VMukti 1.0 -- An Open Source Unified Communications Engine
*
* Copyright (C) 2008 - 2009, VMukti Solutions Pvt. Ltd.
*
* Hardik Sanghvi <hardik@vmukti.com>
*
* See http://www.vmukti.com for more information about
* the VMukti project. Please do not directly contact
* any of the maintainers of this project for assistance;
* the project provides a web site, forums and mailing lists
* for your use.

* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.

* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.

* You should have received a copy of the GNU General Public License
* along with this program. If not, see <http://www.gnu.org/licenses/>

*/
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using FilterDesigner.Common;
using System.Data;
using FilterDesigner.DataAccess;
//using VMuktiAPI;

namespace FilterDesigner.Business
{
   public class ClsTimeZone : ClsBaseObject 
    {
      //public static StringBuilder sb1;
      private Int64 _ID =  ClsConstants.NullLong;
      private string _TimezoneName = ClsConstants.NullString;

       public Int64 ID    
       {
         get { return _ID;}
         set { _ID = value;}
       }
       public string TimezoneName
       {
          get{ return _TimezoneName;}
          set{_TimezoneName=value;}
       }

       //public static StringBuilder CreateTressInfo()
       //{
       //    StringBuilder sb = new StringBuilder();
       //    sb.Append("User Is : " + VMuktiAPI.VMuktiInfo.CurrentPeer.DisplayName);
       //    sb.AppendLine();
       //    sb.Append("Peer Type is : " + VMuktiAPI.VMuktiInfo.CurrentPeer.CurrPeerType.ToString());
       //    sb.AppendLine();
       //    sb.Append("User's SuperNode is : " + VMuktiAPI.VMuktiInfo.CurrentPeer.SuperNodeIP);
       //    sb.AppendLine();
       //    sb.Append("User's Machine Ip Address : " + VMuktiAPI.GetIPAddress.ClsGetIP4Address.GetIP4Address());
       //    sb.AppendLine();
       //    sb.AppendLine("----------------------------------------------------------------------------------------");
       //    return sb;
       //}
       public override bool MapData(DataRow row)
       {
           try
           {
               ID = GetInt(row, "ID");
               TimezoneName = GetString(row, "TimezoneName");
               
              return base.MapData(row);
           }
           catch (Exception ex)
           {
               VMuktiAPI.VMuktiHelper.ExceptionHandler(ex, "MapData()", "ClsTimeZone.cs");
               return false;
           }
       }
       
       
      
    }
}