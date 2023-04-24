// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#include <string>
#include <stdio.h>
#include <vector>

#include "CoreMinimal.h"

class UParameterObject;
class AActor;

void NullParse(std::vector<uint8_t> kMsg, AActor* actor);


class AbstractParameter
{
public:
    bool _distribute;

    // Fallback to NullParse to avoid crash in case no function assigned
    void (*parse)(std::vector<uint8_t> kMsg, AActor* actor) = &NullParse;

    protected:
    short _id;
    std::string _name;
    UParameterObject* _parent;
    AActor* _actor;

public:
    std::string GetName()
    {
        return _name;
    }

    void ParseMessage(std::vector<uint8_t> kMsg)
    {
        if(_actor)
            parse(kMsg, _actor);
    }
};


template <class T>
class Parameter : public AbstractParameter
{
public:
    //Parameter(T value, std::string name, void (*callback_func)(std::vector<uint8_t> kMsg), UParameterObject* parent = null, bool distribute = true)
    Parameter(T value, AActor* actor, std::string name, void (*callback_func)(std::vector<uint8_t> kMsg, AActor* actor), UParameterObject* parent = null, bool distribute = true)
    {
        _value = value;
        _name = name;
        _parent = parent;
        _actor = actor;
        //_type = toVPETType(typeof(T));
        _distribute = distribute;

        //history initialization
        _initialValue = value;

        FString fName(_name.c_str());
        //UE_LOG(LogTemp, Warning, TEXT("Constructor, name %s"), *fName);

        if (parent)
        {
            _id = _parent->GetParameterList()->Num();
            _parent->GetParameterList()->Add((AbstractParameter*)this);
            //UE_LOG(LogTemp, Warning, TEXT("Added param to list, id %d"), _id);
        }
        else
        {
            _id = -1;
            _distribute = false;
        }

        // function
        parse = callback_func;

    }

protected:
    T _value;

private:
    T _initialValue;
};


class ListParameter : Parameter<int>
{
    //!
    //! The ListParamters constructor, initializing members.
    //!
    //! @param parameterList The list of parameders with the given type T.
    //! @param name The parameters name.
    //! @param name The parameters parent ParameterObject.
    //! @param name Flag that determines whether a Parameter will be distributed.
    //!

};