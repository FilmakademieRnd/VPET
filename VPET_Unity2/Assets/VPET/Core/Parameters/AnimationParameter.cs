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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace vpet
{
    //!
    //! RPCParameter class defining the fundamental functionality and interface
    //!
    public class AnimationParameter<T> : Parameter<T>
    {
        //!
        //! The next and the previous active keyframe index (for animation).
        //!
        private int _nextIdx, _prevIdx;
        //!
        //! The list of keyframes (for animation).
        //!
        private List<Key<T>> _keyList = null;
        //!
        //! A reference to the key list (for animation).
        //!
        public ref List<Key<T>> _key_List { get => ref _keyList; }
        //!
        //! A reference to the Animation Manager.
        //!
        private AnimationManager _animationManager = null;
        public AnimationParameter(T parameterValue, string name, ParameterObject parent = null, bool distribute = true) : base(parameterValue, name, parent, distribute) 
        {
            _nextIdx = 0;
            _prevIdx = 0;
        }

        //!
        //! Copy Constructor
        //! @param p source parameter to copy values from
        //!
        public AnimationParameter(AnimationParameter<T> p) : base(p)
        {
            _nextIdx = 0;
            _prevIdx = 0;
            _keyList = p._keyList;
            _animationManager = p._animationManager;

            if (_keyList != null && _animationManager != null)
                _animationManager.animationUpdate += updateValue;
        }

        //!
        //! Destructor
        //!
        ~AnimationParameter()
        {
            clearKeys();
        }

        /////////////////////////////////////////////////////////
        /////////////////////// Animation ///////////////////////
        /////////////////////////////////////////////////////////

        //!
        //! Initializes the parameters animation functionality,
        //!
        private void initAnimation()
        {
            _keyList ??= new List<Key<T>>();

            if (_animationManager == null)
            {
                _animationManager = ParameterObject.core.getManager<AnimationManager>();
                _animationManager.animationUpdate += updateValue;
            }
        }

        //!
        //! Insert a given key element to the parameters key list, at the corresponding index.
        //!
        //! @param key The key to be added to the parameters key list.
        //!
        public void addKey(Key<T> key)
        {
            if (!isAnimated())
                initAnimation();

            int i = findNextKeyIndex(key);
            if (i == -1)
            {
                int i2 = _keyList.IndexOf(key);
                if (i2 > -1)
                    _keyList[i2].value = key.value;
                else
                    _keyList.Add(key);
            }
            else
                _keyList.Insert(i, key);

        }

        //!
        //! Revove a given key element from the parameters key list.
        //!
        //! @param key The key to be removed from the parameters key list.
        //!
        public void removeKey(Key<T> key)
        {
            if (_keyList != null)
            {
                _keyList.Remove(key);
                if (_keyList.Count == 0)
                    _animationManager.animationUpdate -= updateValue;
            }
        }

        //!
        //! Create and insert a new key element to the parameters key list, 
        //! based on the current parameter value and Animation Manager time.
        //!
        public void setKey()
        {
            addKey(new Key<T>(_animationManager.time, value));
        }

        //!
        //! Clear the parameters key list and disable the animation functionality.
        //!
        private void clearKeys()
        {
            if (_animationManager != null)
                _animationManager.animationUpdate -= updateValue;

            if (_keyList != null)
                _keyList.Clear();
        }

        //!
        //! Calculate the parameters value based on the keylist and given time.
        //!
        //! @param o A reference to the Animation Manager.
        //! @param time The given time used to calulate the parameters new value.
        //!
        private void updateValue(object o, float time)
        {
            if (isAnimated())
            {
                // current time is NOT in between the two active keys
                if (time < _keyList[_prevIdx].time || time > _keyList[_nextIdx].time)
                {
                    int i = findNextKeyIndex(time);
                    // current time is bigger than all keys in list
                    if (i == -1)
                        _nextIdx = _prevIdx = _keyList.Count - 1;
                    else
                    {
                        // current time is smaller than all keys in list
                        if (i == 0)
                            _nextIdx = _prevIdx = 0;
                        // current time is somewhere between all keys in list
                        else
                        {
                            _nextIdx = i;
                            _prevIdx = i - 1;
                        }
                    }
                }
                value = interpolate(time);
            }
        }

        //!
        //! Function for searching the next bigger key index in the key list.
        //!
        //! @param key The key on which the index is to be searched.
        //! @return The next bigger index in the keylist.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int findNextKeyIndex(Key<T> key)
        {
            return _keyList.FindIndex(i => i.time > key.time);
        }

        //!
        //! Function for searching the next bigger key index in the key list.
        //!
        //! @param time The time on which the index is to be searched.
        //! @return The next bigger index in the keylist.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int findNextKeyIndex(float time)
        {
            return _keyList.FindIndex(i => i.time >= time);
        }

        //!
        //! Function that returns the current animation state of the parameter.
        //!
        //! @return The current animation state of the parameter.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isAnimated()
        {
            if (_keyList != null)
                return _keyList.Count > 0;
            else
                return false;
        }

        //!
        //! Function that interpolates the current parameter value based on a given
        //! time and the previous and next time indices.
        //!
        //! @parameter time The given time used to interpolate the parameters value.
        //! @return The interpolated parameter value.
        //!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T interpolate(float time)
        {
            switch (_type)
            {
                case ParameterType.FLOAT:
                    float inBetween = (time - _keyList[_prevIdx].time) / (_keyList[_prevIdx].time - _keyList[_nextIdx].time);
                    float s1 = 1.0f - (_keyList[_nextIdx].time - inBetween) / (_keyList[_nextIdx].time - _keyList[_prevIdx].time);
                    return (T)(object)(((float)(object)_keyList[_prevIdx].value) * (1.0f - s1) + ((float)(object)_keyList[_nextIdx].value) * s1);
                default:
                    return default(T);
            }
        }
    }
}