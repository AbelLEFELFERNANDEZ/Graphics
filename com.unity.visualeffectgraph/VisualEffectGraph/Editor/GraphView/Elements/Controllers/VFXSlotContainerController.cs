using System;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Experimental.VFX;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Object = UnityEngine.Object;

namespace UnityEditor.VFX.UI
{
    class VFXSlotContainerController : VFXNodeController
    {
        public VFXSlotContainerController(VFXModel model, VFXViewController viewController) : base(model, viewController)
        {
            var settings = model.GetSettings(true);
            m_Settings = new VFXSettingController[settings.Count()];
            int cpt = 0;
            foreach (var setting in settings)
            {
                var settingController = new VFXSettingController();
                settingController.Init(this.slotContainer, setting.Name, setting.FieldType);
                m_Settings[cpt++] = settingController;
            }
        }

        protected override void ModelChanged(UnityEngine.Object obj)
        {
            var inputs = inputPorts;
            List<VFXDataAnchorController> newAnchors = new List<VFXDataAnchorController>();

            UpdateSlots(newAnchors, slotContainer.inputSlots, true, true);

            foreach (var anchorController in m_InputPorts.Except(newAnchors))
            {
                anchorController.OnDisable();
            }
            m_InputPorts = newAnchors;
            newAnchors = new List<VFXDataAnchorController>();
            UpdateSlots(newAnchors, slotContainer.outputSlots, true, false);

            foreach (var anchorController in m_OutputPorts.Except(newAnchors))
            {
                anchorController.OnDisable();
            }
            m_OutputPorts = newAnchors;

            base.ModelChanged(obj);
        }

        public override VFXSlotContainerController slotContainerController { get { return this; } }


        void UpdateSlots(List<VFXDataAnchorController> newAnchors, IEnumerable<VFXSlot> slotList, bool expanded, bool input)
        {
            VFXSlot[] slots = slotList.ToArray();
            {
                foreach (VFXSlot slot in slots)
                {
                    VFXDataAnchorController propController = GetPropertyController(slot, input);

                    if (propController == null)
                    {
                        propController = AddDataAnchor(slot, input, !expanded);
                    }
                    newAnchors.Add(propController);

                    if (!VFXDataAnchorController.SlotShouldSkipFirstLevel(slot))
                    {
                        UpdateSlots(newAnchors, slot.children, expanded && !slot.collapsed, input);
                    }
                    else
                    {
                        VFXSlot firstSlot = slot.children.First();
                        UpdateSlots(newAnchors, firstSlot.children, expanded && !slot.collapsed, input);
                    }
                }
            }
        }

        public VFXDataAnchorController GetPropertyController(VFXSlot slot, bool input)
        {
            VFXDataAnchorController result = null;

            if (input)
                result = inputPorts.Cast<VFXDataAnchorController>().Where(t => t.model == slot).FirstOrDefault();
            else
                result = outputPorts.Cast<VFXDataAnchorController>().Where(t => t.model == slot).FirstOrDefault();

            return result;
        }

        protected virtual VFXDataAnchorController AddDataAnchor(VFXSlot slot, bool input, bool hidden)
        {
            return null;
        }

        public IVFXSlotContainer slotContainer { get { return model as IVFXSlotContainer; } }

        public VFXSettingController[] settings
        {
            get { return m_Settings; }
        }

        public virtual bool enabled
        {
            get { return true; }
        }

        public new bool expanded
        {
            get
            {
                return !slotContainer.collapsed;
            }

            set
            {
                if (value != !slotContainer.collapsed)
                {
                    slotContainer.collapsed = !value;
                }
            }
        }

        public virtual void DrawGizmos(VFXComponent component)
        {
            foreach (VFXDataAnchorController controller in inputPorts.Cast<VFXDataAnchorController>())
            {
                controller.DrawGizmo(component);
            }
        }

        public override IEnumerable<Controller> allChildren
        {
            get { return base.allChildren.Concat(m_Settings.Cast<Controller>()); }
        }

        private VFXSettingController[] m_Settings;
    }
}
