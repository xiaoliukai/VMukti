/* VMukti 2.0 -- An Open Source Video Communications Suite
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
using ScriptDesigner.Common;
using System.Data;
using VMuktiAPI;

namespace ScriptDesigner.Business
{
    public class ClsCampaign : ClsBaseObject
    {
        #region Fields

        private Int64 _ID = ScriptDesigner.Common.ClsConstants.NullLong;
        private string _Name = ScriptDesigner.Common.ClsConstants.NullString;
        private bool _IsActive = false;
        private int _DispositionListID = ScriptDesigner.Common.ClsConstants.NullInt;
        
        #endregion 

        #region Properties

        public Int64 ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public bool IsActive
        {
            get { return _IsActive; }
            set { _IsActive = value; }
        }

        public int DispositionListID
        {
            get { return _DispositionListID; }
            set { _DispositionListID = value; }
        }

        #endregion 

        #region Methods

        public override bool MapData(DataRow row)
        {
            try
            {
                ID = GetLong(row, "ID");
                Name = GetString(row, "Name");
                IsActive = GetBool(row, "IsActive");
                DispositionListID = GetInt(row, "DispositionListID");
                
                return base.MapData(row);
            }
            catch (Exception ex)
            {
                VMuktiHelper.ExceptionHandler(ex, "MapObjects", "ClsCampaign.cs");
                return false;
            }
        }

        #endregion 
    }
}
