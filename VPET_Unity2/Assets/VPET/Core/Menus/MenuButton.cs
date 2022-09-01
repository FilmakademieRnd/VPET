/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright(c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "MenuButton.cs"
//! @brief Implementation of the VPET UI Menu Tree, serving as internal structure to reflect menus.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 21.01.2022

using System;
using System.Collections.Generic;

namespace vpet
{
    public class MenuButton
    {
        public int id = -1;
        public bool isToggle = false;
        //!
        //! Class defining button highlighting event arguments.
        //!
        public class HighlightEventArgs : EventArgs
        {
            public int id;
            public bool highlight;
        }
        public event EventHandler<HighlightEventArgs> m_highlightEvent;
        protected List<UIManager.Roles> m_roles = new List<UIManager.Roles>();
        public ref List<UIManager.Roles> roles
        {
            get => ref m_roles;
        }
        private Action m_action;
        public Action action
        {
            get => m_action;
            set => m_action = value;
        }
        //!
        //! The name and caption of a menu.
        //!
        private string m_caption;
        public string caption
        {
            set => m_caption = value;
            get => m_caption;
        }
        //!
        //! The optional icon for an menu.
        //!
        protected string m_iconResourceLocation = "";
        public string iconResourceLocation 
        {
            set => m_iconResourceLocation = value;
            get => m_iconResourceLocation; 
        }

        public MenuButton(string caption = "", Action action = null, List<UIManager.Roles> roles = null)
        {
            m_caption = caption;
            m_action = action;
            if (roles != null)
                m_roles.AddRange(roles);
        }

        public void setIcon(string resourceLocation)
        {
            m_iconResourceLocation = resourceLocation;
        }

        public void showHighlighted(bool highlighted)
        {
            HighlightEventArgs h = new HighlightEventArgs();
            h.id = id;
            h.highlight = highlighted;
            m_highlightEvent?.Invoke(this, h);
        }
    }

}
