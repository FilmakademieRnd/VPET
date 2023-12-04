/*
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.

Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project SAUCE (https://www.sauceproject.eu/)
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
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
        //! The key's value and tangent value.
        //!
        public T value, tangentValue;
        //!
        //! The key's time and tangent time.
        //!
        public float time, tangentTime;
        //!
        //! Enumeration for the different interpolation types
        //!
        public enum KeyType { STEP, LINEAR, BEZIER }
        //!
        //! The key's type.
        //!
        public KeyType type;

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