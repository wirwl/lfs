#region License
// Copyright © Pavel Fedarovich, 2010-2012
// 
// This file is part of AvalonWizard.
//  
// You may at your option receive a license to Avalon Wizard under 
// either the terms of the Apache License version 2.0 or 
// the GNU Lesser General Public License (LGPL) version 2.1 or any later version.
//  
// AvalonWizard is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
//  
// You may obtain a copy of the Apache License at [http://www.apache.org/licenses/LICENSE-2.0].
// You may obtain a copy of the LGPL at [http://www.gnu.org/licenses/].
#endregion

using System.Windows.Input;
using AvalonWizard.Extensions;

namespace AvalonWizard
{
    internal class ExtendedMouseGesture : MouseGesture
    {
        private readonly ExtendedMouseAction extendedMouseAction;

        public ExtendedMouseGesture(ExtendedMouseAction mouseAction) : 
            this(mouseAction, ModifierKeys.None)
        {
        }

        public ExtendedMouseGesture(ExtendedMouseAction mouseAction, ModifierKeys modifiers)
            : base(ConvertExtendedMouseAction(mouseAction), modifiers)
        {
            extendedMouseAction = mouseAction;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var action = GetExtendedMouseAction(inputEventArgs);
            if (action != null && extendedMouseAction == action.Value)
            {
                return Modifiers == Keyboard.Modifiers;
            }
            return base.Matches(targetElement, inputEventArgs);
        }

        private static MouseAction ConvertExtendedMouseAction(ExtendedMouseAction action)
        {
            return action.In(ExtendedMouseAction.XButton1Click, ExtendedMouseAction.XButton2Click)
                       ? MouseAction.None
                       : (MouseAction) action;
        }

        private static ExtendedMouseAction? GetExtendedMouseAction(InputEventArgs inputEventArgs)
        {
            var mouseButtonEventArgs = inputEventArgs as MouseButtonEventArgs;
            if (mouseButtonEventArgs != null)
            {
                switch (mouseButtonEventArgs.ChangedButton)
                {
                    case MouseButton.XButton1:
                        return ExtendedMouseAction.XButton1Click;
                    case MouseButton.XButton2:
                        return ExtendedMouseAction.XButton2Click;
                }
            }
            return null;
        }
    }
}
