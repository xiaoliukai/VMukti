﻿#pragma checksum "..\..\..\Controls\wndVMuktiPopup.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7ECE18FD72B29C60F44EF0F6A671B7CA"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3082
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace VMukti.Presentation.Controls {
    
    
    /// <summary>
    /// wndVMuktiPopup
    /// </summary>
    public partial class wndVMuktiPopup : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 7 "..\..\..\Controls\wndVMuktiPopup.xaml"
        internal VMukti.Presentation.Controls.wndVMuktiPopup window;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\Controls\wndVMuktiPopup.xaml"
        internal System.Windows.Controls.Border brdmain;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\..\Controls\wndVMuktiPopup.xaml"
        internal System.Windows.Controls.TextBlock tblHost;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\..\Controls\wndVMuktiPopup.xaml"
        internal System.Windows.Controls.TextBlock tblModule;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\Controls\wndVMuktiPopup.xaml"
        internal System.Windows.Controls.TextBlock tblParticipants;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/VMukti.Presentation;component/controls/wndvmuktipopup.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Controls\wndVMuktiPopup.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.window = ((VMukti.Presentation.Controls.wndVMuktiPopup)(target));
            return;
            case 2:
            this.brdmain = ((System.Windows.Controls.Border)(target));
            return;
            case 3:
            this.tblHost = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.tblModule = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.tblParticipants = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}