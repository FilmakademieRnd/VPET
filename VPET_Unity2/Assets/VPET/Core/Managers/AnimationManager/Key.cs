/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
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

//! @file "key.cs"
//! @brief Implementation of the vpet parameter key
//! @author Simon Spielmann
//! @version 0
//! @date 22.08.2022


namespace vpet
{

    //!
    //! Parameter base class.
    //!
    public class Key<T>
    {
        //!
        //! Enumeration for the different interpolation types
        //!
        public enum KeyType { STEP, LINEAR, BEZIER }
        //!
        //! The key's type.
        //!
        public KeyType type;
        //!
        //! The key's time and tangent time.
        //!
        public float time, tangentTime;
        //!
        //! The key's value and tangent value.
        //!
        public T value, tangentValue;

        //!
        //! The Key's constructor for generic types.
        //!
        public Key(float time, T value, float tangentTime = 0, T tangentValue = default(T), KeyType type = KeyType.LINEAR)
        {
            this.time = time;
            this.value = value;
            this.tangentTime = tangentTime;
            this.tangentValue = tangentValue;
            this.type = KeyType.LINEAR;
        }

        //!
        //! The Key's constructor for type LINEAR.
        //!
        public Key (float time, T value)
        {
            this.time = time;
            this.value = value;
            this.tangentTime = 0;
            this.tangentValue = default(T);
            this.type = KeyType.LINEAR;
        }
    }
}