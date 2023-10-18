/*
TRACER FOUNDATION - 
Toolset for Realtime Animation, Collaboration & Extended Reality
tracer.research.animationsinstitut.de
https://github.com/FilmakademieRnd/TRACER

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

TRACER is a development by Filmakademie Baden-Wuerttemberg, Animationsinstitut
R&D Labs in the scope of the EU funded project MAX-R (101070072) and funding on
the own behalf of Filmakademie Baden-Wuerttemberg.  Former EU projects Dreamspace
(610005) and SAUCE (780470) have inspired the TRACER development.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.
You should have received a copy of the MIT License along with this program; 
if not go to https://opensource.org/licenses/MIT
*/

//! @file "MenuButton.cs"
//! @brief Implementation of the TRACER UI Menu Tree, serving as internal structure to reflect menus.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 21.01.2022

using System;
using System.Collections.Generic;

namespace tracer
{
    public class MenuButton
    {
        private static int s_id = 0;
        //!
        //! The unique id of the MenuButton.
        //!
        public int id = -1;
        //!
        //! Flag that determines wether a MenuButton is a toggle or not.
        //!
        public bool isToggle = false;
        //!
        //! Flag that determines wether a MenuButton is highlighted in UI or not.
        //!
        public bool isHighlighted = false;
        //!
        //! Class defining button highlighting event arguments.
        //!
        public class HighlightEventArgs : EventArgs
        {
            public int id;
            public bool highlight;
        }
        //!
        //! Event invoked when the MenuButtons highligted status changed.
        //!
        public event EventHandler<HighlightEventArgs> m_highlightEvent;
        //!
        //! List containung the roles for which the element is visible in the UI. 
        //!
        protected List<UIManager.Roles> m_roles = new List<UIManager.Roles>();
        public ref List<UIManager.Roles> roles
        { get => ref m_roles; }
        //!
        //! Optional action that will be executed when the button is clicked.
        //!
        private Action m_action;
        public Action action
        {
            get => m_action;
            set => m_action = value;
        }
        //!
        //! The name and caption of a MenuButton.
        //!
        private string m_caption;
        public string caption
        {
            set => m_caption = value;
            get => m_caption;
        }
        
        public string name { get; }
        
        //!
        //! The optional icon for an MenuButton.
        //!
        protected string m_iconResourceLocation = "";
        public string iconResourceLocation 
        {
            set => m_iconResourceLocation = value;
            get => m_iconResourceLocation; 
        }
        //!
        //! Constructor
        //!
        public MenuButton(string caption = "", Action action = null, List<UIManager.Roles> roles = null, string name = "")
        {
            this.name = name;
            id = s_id++;
            m_caption = caption;
            m_action = action;
            if (roles != null)
                m_roles.AddRange(roles);
        }
        //!
        //! Sets the icon location of a MenuButton
        //!
        //! @param resourceLocation The icon location.
        //!
        public void setIcon(string resourceLocation)
        {
            m_iconResourceLocation = resourceLocation;
        }
        //!
        //! Sets the MenuButtons highlight state.
        //!
        //! @param highlighted The new highlight state to be set.
        //!
        public void showHighlighted(bool highlighted)
        {
            HighlightEventArgs h = new HighlightEventArgs();
            h.id = id;
            h.highlight = highlighted;
            m_highlightEvent?.Invoke(this, h);
        }
    }

}
