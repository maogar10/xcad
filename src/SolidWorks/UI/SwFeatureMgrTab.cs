﻿//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Exceptions;
using System.Linq;

namespace Xarial.XCad.SolidWorks.UI
{
    public interface ISwFeatureMgrTab<TControl> : IXCustomPanel<TControl>, IDisposable 
    {
        IFeatMgrView FeatureManagerView { get; }
    }
    
    internal class SwFeatureMgrTab<TControl> : DocumentAttachedCustomPanel<TControl>, ISwFeatureMgrTab<TControl> 
    {
        public IFeatMgrView FeatureManagerView
        {
            get
            {
                if (IsControlCreated)
                {
                    return m_CurFeatMgrView;
                }
                else
                {
                    throw new CustomPanelControlNotCreatedException();
                }
            }
        }

        private IModelViewManager m_ModelViewMgr;
        private IFeatMgrView m_CurFeatMgrView;
        private readonly FeatureManagerTabCreator<TControl> m_CtrlCreator;
        private string m_Title;
        private int m_TabIndex;

        internal SwFeatureMgrTab(FeatureManagerTabCreator<TControl> ctrlCreator, SwDocument doc, ISwApplication app, IXLogger logger)
            : base(doc, app, logger)
        {
            m_ModelViewMgr = doc.Model.ModelViewManager;
            m_CtrlCreator = ctrlCreator;

            switch (doc.Model) 
            {
                case PartDoc part:
                    part.FeatureManagerTabActivatedNotify += OnFeatureManagerTabActivated;
                    break;

                case AssemblyDoc assm:
                    assm.FeatureManagerTabActivatedNotify += OnFeatureManagerTabActivated;
                    break;

                case DrawingDoc drw:
                    drw.FeatureManagerTabActivatedNotify += OnFeatureManagerTabActivated;
                    break;
            }
        }

        protected override bool GetIsActive() => m_ModelViewMgr.ActiveFeatureManagerTabIndex == m_TabIndex;

        protected override void SetIsActive(bool active)
        {
            if (active)
            {
                m_CurFeatMgrView.ActivateView();
            }
            else 
            {
                if (!m_CurFeatMgrView.DeActivateView()) 
                {
                    m_Logger.Log("Failed to deactivate view", XCad.Base.Enums.LoggerMessageSeverity_e.Error);
                }
            }
        }
        
        protected override TControl CreateControl()
        {
            var ctrlData = m_CtrlCreator.CreateControl(typeof(TControl), out TControl ctrl);
            
            m_CurFeatMgrView = ctrlData.Item1;
            m_Title = ctrlData.Item2;

            m_TabIndex = Array.LastIndexOf(m_Doc.Model.ModelViewManager.GetFeatureManagerTabs() as string[], m_Title);

            return ctrl;
        }

        protected override void DeleteControl()
        {
            if (!FeatureManagerView.DeleteView())
            {
                m_Logger.Log("Failed to delete feature manager view", XCad.Base.Enums.LoggerMessageSeverity_e.Error);
            }

            switch (m_Doc.Model)
            {
                case PartDoc part:
                    part.FeatureManagerTabActivatedNotify -= OnFeatureManagerTabActivated;
                    break;

                case AssemblyDoc assm:
                    assm.FeatureManagerTabActivatedNotify -= OnFeatureManagerTabActivated;
                    break;

                case DrawingDoc drw:
                    drw.FeatureManagerTabActivatedNotify -= OnFeatureManagerTabActivated;
                    break;
            }
        }

        private int OnFeatureManagerTabActivated(int commandIndex, string commandTabName)
        {
            if (commandIndex == m_TabIndex) 
            {
                RaiseActivated();
            }

            return 0;
        }
    }
}
