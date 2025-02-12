﻿//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Binders;
using Xarial.XCad.Utils.PageBuilder.Constructors;
using Xarial.XCad.Utils.PageBuilder.Core;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Toolkit.Tests
{
    public class PageBuilderTests
    {
        #region Mocks

        public class ControlMock : Control<object>
        {
            public override bool Enabled { get; set; }
            public override bool Visible { get; set; }

#pragma warning disable CS0067
            protected override event ControlValueChangedDelegate<object> ValueChanged;
#pragma warning restore

            public ControlMock(int id, object tag) : base(id, tag)
            {
            }

            protected override object GetSpecificValue()
            {
                return null;
            }

            protected override void SetSpecificValue(object value)
            {
            }
        }

        public class GroupMock : Group
        {
            public GroupMock(int id, object tag) : base(id, tag)
            {
            }

            public override bool Enabled { get; set; }
            public override bool Visible { get; set; }
        }

        public class PageMock : Page
        {
            public List<ControlMock> Controls { get; } = new List<ControlMock>();

            public override bool Enabled { get; set; }
            public override bool Visible { get; set; }
        }

        [DefaultType(typeof(SpecialTypes.AnyType))]
        public class ControlMockConstructor : PageElementConstructor<ControlMock, GroupMock, PageMock>
        {
            private readonly Func<int> m_IdRangeSelector;

            public ControlMockConstructor(Func<int> idRangeSelector = null)
            {
                m_IdRangeSelector = idRangeSelector;
            }

            private ControlMock Create(PageMock page, IAttributeSet atts, IMetadata metadata)
            {
                var ctrl = new ControlMock(atts.Id, atts.Tag);
                page.Controls.Add(ctrl);
                return ctrl;
            }

            private ControlMock Create(GroupMock group, IAttributeSet atts, IMetadata metadata)
            {
                return new ControlMock(atts.Id, atts.Tag);
            }

            protected override ControlMock Create(PageMock page, IAttributeSet atts, IMetadata metadata, ref int numberOfUsedIds)
            {
                if (m_IdRangeSelector != null)
                {
                    numberOfUsedIds = m_IdRangeSelector.Invoke();
                }

                return Create(page, atts, metadata);
            }

            protected override ControlMock Create(GroupMock group, IAttributeSet atts, IMetadata metadata, ref int numberOfUsedIds)
            {
                if (m_IdRangeSelector != null)
                {
                    numberOfUsedIds = m_IdRangeSelector.Invoke();
                }

                return Create(group, atts, metadata);
            }
        }

        public class PageMockConstructor : PageConstructor<PageMock>
        {
            protected override PageMock Create(IAttributeSet atts)
            {
                return new PageMock();
            }
        }

        public class PageBuilderMock : Xarial.XCad.Utils.PageBuilder.PageBuilderBase<PageMock, GroupMock, ControlMock>
        {
            public PageBuilderMock(Func<int> idRangeSelector = null)
                : base(new Moq.Mock<IXApplication>().Object,
                      new TypeDataBinder(new Mock<IXLogger>().Object), 
                      new PageMockConstructor(),
                      new ControlMockConstructor(idRangeSelector))
            {
            }
        }

        public class DataModel1
        {
            public string Prp1 { get; set; }
            public string Prp2 { get; set; }
            public string Prp3 { get; set; }
        }

        #endregion

        [Test]
        public void CreatePageIdsTest()
        {
            var builder = new PageBuilderMock();
            var page = builder.CreatePage<DataModel1>(x => null);

            Assert.AreEqual(3, page.Controls.Count);
            Assert.AreEqual(0, page.Controls[0].Id);
            Assert.AreEqual(1, page.Controls[1].Id);
            Assert.AreEqual(2, page.Controls[2].Id);
        }

        [Test]
        public void CreatePageIdsRangeTest()
        {
            int ctrlIndex = 0;
            var builder = new PageBuilderMock(()=> 
            {
                int idRange = 1;

                if (ctrlIndex == 1)
                {
                    idRange = 3;
                }
                
                ctrlIndex++;
                return idRange;
            });
            var page = builder.CreatePage<DataModel1>(x => null);

            Assert.AreEqual(3, page.Controls.Count);
            Assert.AreEqual(0, page.Controls[0].Id);
            Assert.AreEqual(1, page.Controls[1].Id);
            Assert.AreEqual(4, page.Controls[2].Id);
        }
    }
}
