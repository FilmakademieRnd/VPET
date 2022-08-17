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

//! @file "Dialog.cs"
//! @brief Implementation of the VPET dialog, serving as internal structure to reflect dialogs.
//! @author Simon Spielmann
//! @version 0
//! @date 21.08.2022

using System;

namespace vpet
{
    public class Dialog
    {
        //!
        //! Enumeration of all suppoted dialog types.
        //!
        public enum DTypes
        {
            ERROR, WARNING, INFO, BAR
        }
        //!
        //! The actual type of the dialog.
        //!
        private DTypes m_type = DTypes.INFO;
        public DTypes type
        { get => m_type; }
        //!
        //! The actual role of the dialog.
        //!
        protected UIManager.Roles m_role;
        public UIManager.Roles role
        { get => m_role; }
        //!
        //! The caption of the dialog.
        //!
        private string m_caption = "";
        public string caption
        {   
            set 
            {
                m_caption = value;
                captionChanged?.Invoke(this, value);
            }
            get => m_caption; 
        }
        //!
        //! The message of the dialog.
        //!
        private string m_message = "";
        public string message
        { get => m_message; }
        //!
        //! The progress of the dialog.
        //!
        private int m_progress = 0;
        public int progress
        {
            get
            {
                return m_progress;
            }
            set 
            {
                m_progress = value;
                progressChanged?.Invoke(this, value);
            }
        }

        //!
        //! Event that is invoked when the dialos progress changed.
        //!
        public event EventHandler<int> progressChanged;
        //!
        //! Event that is invoked when the dialos caption changed.
        //!
        public event EventHandler<string> captionChanged;

        //!
        //! Constructor of the dialog class.
        //!
        //! @param caption The caption of the dialog.
        //! @param message The message of the dialog.
        //! @param type The type of the dialog.
        //! @param role The role of the dialog.
        //!
        public Dialog(string caption, string message, DTypes type, UIManager.Roles role = UIManager.Roles.EXPERT)
        {
            m_caption = caption;
            m_message = message;
            m_type = type;
            m_role = role;
        }
        //!
        //! Constructor of the dialog class.
        //!
        public Dialog(string message, DTypes type, UIManager.Roles role = UIManager.Roles.EXPERT)
        {
            if (type != DTypes.BAR)
            {
                m_caption = type.ToString();
                m_message = message;
            }
            m_type = type;
            m_role = role;
        }
        //!
        //! Constructor of the dialog class.
        //!
        public Dialog (DTypes type = DTypes.BAR, UIManager.Roles role = UIManager.Roles.EXPERT) : this("", type, role)
        {
        }

    }

}
