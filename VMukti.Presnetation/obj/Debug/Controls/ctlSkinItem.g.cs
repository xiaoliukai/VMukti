﻿#pragma checksum "..\..\..\Controls\ctlSkinItem.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "C60067E59FEB3021C6B2A27EECB99E46"
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
    /// ctlSkinItem
    /// </summary>
    public partial class ctlSkinItem : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 5 "..\..\..\Controls\ctlSkinItem.xaml"
        internal VMukti.Presentation.Controls.ctlSkinItem UserControl;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\Controls\ctlSkinItem.xaml"
        internal System.Windows.Media.Animation.BeginStoryboard OnMouseEnter_Copy1_BeginStoryboard;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\Controls\ctlSkinItem.xaml"
        internal System.Windows.Controls.Border border;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\Controls\ctlSkinItem.xaml"
        internal System.Windows.Controls.Grid LayoutRoot;
        
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
            System.Uri resourceLocater = new System.Uri("/VMukti.Presentation;component/controls/ctlskinitem.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Controls\ctlSkinItem.xaml"
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
            this.UserControl = ((VMukti.Presentation.Controls.ctlSkinItem)(target));
            return;
            case 2:
            this.OnMouseEnter_Copy1_BeginStoryboard = ((System.Windows.Media.Animation.BeginStoryboard)(target));
            return;
            case 3:
            this.border = ((System.Windows.Controls.Border)(target));
            return;
            case 4:
            this.LayoutRoot = ((System.Windows.Controls.Grid)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}