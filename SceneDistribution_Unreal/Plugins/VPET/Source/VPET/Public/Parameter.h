// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#include <string>
#include <stdio.h>
#include <vector>

//#include "ParameterObject.h"

#include "CoreMinimal.h"

class UParameterObject;
class AActor;

//class USceneObject;

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
    // temporary "hack"
    AActor* _actor;

    //void copyValue(AbstractParameter v);
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

class Test : AbstractParameter
{


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

template <class T>
class OldParameter : public AbstractParameter
{
public:

    OldParameter(T value, std::string name, UParameterObject* parent = null, bool distribute = true)
    {
        _value = value;
        _name = name;
        _parent = parent;
        //_type = toVPETType(typeof(T));
        _distribute = distribute;

        //history initialization
        _initialValue = value;

        FString fName(_name.c_str());
        UE_LOG(LogTemp, Warning, TEXT("Constructor, name %s"), *fName);
        
        if (parent)
        {
        //    _id = (short)_parent.parameterList.Count;
            _parent->GetParameterList()->Add((AbstractParameter*)this);
            UE_LOG(LogTemp, Warning, TEXT("Added param to list"));
            //Debug.Log("Added param " + this.name);
        }
        //else
        //{
        //    _id = -1;
        //    _distribute = false;
        //}

    }

    void MainFunction()
    {
        //CSource source;

        //// call for events
        //UE_LOG(LogTemp, Warning, TEXT("PREP EVENTS 2"));
        //hookEvent(&source);
        //__raise source.MyEvent(123);
        //unhookEvent(&source);
        //UE_LOG(LogTemp, Warning, TEXT("EVENTS DONE 2"));
        //CSource source;
        //CReceiver receiver;

        //receiver.hookEvent(&source);
        //__raise source.MyEvent(123);
        FString fString(_name.c_str());
        UE_LOG(LogTemp, Warning, TEXT("Param function from %s"), *fString);
        //receiver.unhookEvent(&source);
    }

    //void MyEvenHandler(int nValue) {
    //    printf_s("MyHandler was called with value %d.\n", nValue);
    //}

    //void hookEvent(CSource* pSource) {
    //    __hook(&CSource::MyEvent, pSource, &this::MyEvenHandler);
    //}

    //void unhookEvent(CSource* pSource) {
    //    __unhook(&CSource::MyEvent, pSource, &this::MyEvenHandler);
    //}

protected:
    T _value;

private:
    T _initialValue;
};

template <class T>
class FunParameter : public AbstractParameter
{
public:

    void (*foo)(std::vector<uint8_t> kMsg);

    //int operation2(int x, int y, std::function<void()> function)
    //{ return function(); }

    //FunParameter(T value, std::string name, void *callback_func)
    //FunParameter(T value, std::string name, std::function<void()> callback_func)
    FunParameter(T value, std::string name, void (*callback_func)(std::vector<uint8_t> kMsg), UParameterObject* parent = null)
    {
        _value = value;
        _name = name;
        _parent = parent;

        //history initialization
        _initialValue = value;

        FString fName(_name.c_str());
        UE_LOG(LogTemp, Warning, TEXT("Fun constructor, name %s"), *fName);

        if (parent)
        {
            //    _id = (short)_parent.parameterList.Count;
            _parent->GetParameterList()->Add((AbstractParameter*)this);
            UE_LOG(LogTemp, Warning, TEXT("Added param to list"));
            //Debug.Log("Added param " + this.name);
        }

        // addresss
        foo = callback_func;

        parse = callback_func;

    }

    void MainFunction()
    {
        std::vector<uint8_t> kMsg;
        kMsg.clear();
        kMsg.push_back(1);
        kMsg.push_back(2);
        kMsg.push_back(3);
        kMsg.push_back(4);
        kMsg.push_back(5);
        kMsg.push_back(6);
        kMsg.push_back(7);
        kMsg.push_back(8);
        foo(kMsg);

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


//class VPET427_API UParameter
//{
//	GENERATED_BODY()
//
//public:	
//	// Sets default values for this component's properties
//	//UParameterObject();
//
//protected:
//	// Called when the game starts
//	//virtual void BeginPlay() override;
//
//public:	
//	// Called every frame
//	//virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;
//	
//};

