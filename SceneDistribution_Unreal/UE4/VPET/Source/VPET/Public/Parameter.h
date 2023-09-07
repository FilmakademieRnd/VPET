// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#include <string>
#include <stdio.h>
#include <vector>
#include <typeindex>
#include <mutex>

#include "CoreMinimal.h"

namespace std
{
    class type_index;
}

class UParameterObject;
class AActor;

void NullParse(std::vector<uint8_t> kMsg, AActor* actor);

//DECLARE_DYNAMIC_MULTICAST_DELEGATE(FVpet_Delegate);

class AbstractParameter
{
public:
    virtual ~AbstractParameter() = default;
    bool _distribute;
    int16_t _dataSize = -1;

    virtual int dataSize() = 0;
    
    std::mutex mtx;
    
    UPROPERTY(BlueprintAssignable)
    //FVpet_Delegate HasChanged;

    // Fallback to NullParse to avoid crash in case no function assigned
    void (*parse)(std::vector<uint8_t> kMsg, AActor* actor) = &NullParse;

    // Definition of VPETs parameter types
    enum class ParameterType : uint8_t { NONE, ACTION, BOOL, INT, FLOAT, VECTOR2, VECTOR3, VECTOR4, QUATERNION, COLOR, STRING, LIST, UNKNOWN = 100 };

    ParameterType _type;

    // unreat types to Vpet types
    static ParameterType toVPETType(const std::type_info& t)
    {
        if (t == typeid(void))
            return ParameterType::NONE;
        else if (t == typeid(TFunction<void()>))
            return ParameterType::ACTION;
        else if (t == typeid(bool))
            return ParameterType::BOOL;
        else if (t == typeid(int32))
            return ParameterType::INT;
        else if (t == typeid(float))
            return ParameterType::FLOAT;
        else if (t == typeid(FVector2D))
            return ParameterType::VECTOR2;
        else if (t == typeid(FVector))
            return ParameterType::VECTOR3;
        else if (t == typeid(FVector4))
            return ParameterType::VECTOR4;
        else if (t == typeid(FQuat))  
            return ParameterType::QUATERNION;
        else if (t == typeid(FColor))
            return ParameterType::COLOR;
        else if (t == typeid(FString))
            return ParameterType::STRING;
        else if (t == typeid(TArray<int32>))  
            return ParameterType::LIST;
        else
            return ParameterType::UNKNOWN;
    }
    
    static std::vector<std::type_index> _paramTypes;

    short _id;
    UParameterObject* _parent;

    
    protected:
    std::string _name;
    AActor* _actor = nullptr;

public:
    std::string GetName()
    {
        return _name;
    }

    ParameterType GetVPetType()
    {
        return _type;
    }

    void ParseMessage(std::vector<uint8_t> kMsg)
    {
        if(_actor)
            parse(kMsg, _actor);
    }

    virtual void Serialize(char* data, std::string name) = 0;
};

/*std::vector<std::type_index> AbstractParameter::paramTypes;*/

template <class T>
class Parameter : public AbstractParameter
{
public:
        virtual int dataSize() override {
            return _dataSize;
    }

    
    //Parameter(T value, std::string name, void (*callback_func)(std::vector<uint8_t> kMsg), UParameterObject* parent = null, bool distribute = true)
    Parameter(T value, AActor* actor, std::string name, void (*callback_func)(std::vector<uint8_t> kMsg, AActor* actor), UParameterObject* parent = null, bool distribute = true)
    {
        _value = value;
        _name = name;
        _parent = parent;
        _actor = actor;
        _type = toVPETType(typeid(T));
        _distribute = distribute;

        //history initialization
        _initialValue = value;

        //Set _dataSize based on the Parameter VPET type 
        switch (_type)
        {
        case ParameterType::NONE:
            _dataSize = 0;
            break;
        case ParameterType::BOOL:
            _dataSize = 1;
            break;
        case ParameterType::INT:
        case ParameterType::FLOAT:
            _dataSize = 4;
            break;
        case ParameterType::VECTOR2:
            _dataSize = 8;
            break;
        case ParameterType::VECTOR3:
            _dataSize = 12;
            break;
        case ParameterType::VECTOR4:
        case ParameterType::QUATERNION:
        case ParameterType::COLOR:
            _dataSize = 16;
            break;
        default:
            _dataSize = -1;
            break;
        }

        //FString fName(_name.c_str());
        //UE_LOG(LogTemp, Warning, TEXT("Constructor, name %s"), *fName);

        if (parent)
        {
            _id = _parent->GetParameterList()->Num();
            _parent->GetParameterList()->Add(this);
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

    
    void setValue(const T& newValue) {
        _value = newValue;
    }

    T getValue() const {
        return _value;
    }

    //Serialize the mesege 
     virtual void Serialize(char* data, std::string name) override
    {
       
        /*switch (_type)
        {
        case ParameterType::BOOL:
            {
 
            }
        case ParameterType::INT:
            {

            }
        case ParameterType::FLOAT:
            {

            }
        case ParameterType::VECTOR2:
            {

            }
        case ParameterType::VECTOR3:
            {
              
            }
        case ParameterType::VECTOR4:
            {

            }

        case ParameterType::QUATERNION:
            {

            }

        case ParameterType::COLOR:
            {

            }

        case ParameterType::STRING:
            {

            }
            
            default:
                break;
        }*/
    }




    
protected:
    T _value;


private:
    T _initialValue;




};

template<>
void Parameter<bool>::Serialize(char* data, std::string name) {

    bool val = _value;
    std::memcpy(data, &val, sizeof(bool));
}

template<>
void Parameter<int32>::Serialize(char* data, std::string name) {

    int32 val = _value;
    std::memcpy(data, &val, sizeof(int32));
}

template<>
void Parameter<float>::Serialize(char* data, std::string name) {

    float val = _value;
    std::memcpy(data, &val, sizeof(float));
    
}

template<>
void Parameter<FVector2D>::Serialize(char* data, std::string name) {

    FVector2D vec2 = _value;
    float x, y;
    x = vec2.X;
    y = vec2.Y;
    std::memcpy(data, &x, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &y, sizeof(float));
}

template<>
void Parameter<FVector>::Serialize(char* data, std::string name) {

    FVector vec3 = _value;
    float x, y, z;
    x = vec3.X;
    y = vec3.Z;
    z = vec3.Y;
    //UE_LOG(LogTemp, Warning, TEXT("FVector values before memcpy inside param.h: %f %f %f"), vec3.X, vec3.Y, vec3.Z);
    if (name == "position")
    {
        x = vec3.X * -0.01;
        y = vec3.Z * 0.01;
        z = vec3.Y * 0.01;
    }

    std::memcpy(data, &x, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &y, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &z, sizeof(float));

}

template<>
void Parameter<FVector4>::Serialize(char* data, std::string name) {

    FVector4 vec4 = _value;
    float x, y, z, w;
    x = vec4.X;
    y = vec4.Y;
    z = vec4.Z;
    w = vec4.W;

    std::memcpy(data, &x, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &y, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &z, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &w, sizeof(float));
}


template<>
void Parameter<FQuat>::Serialize(char* data, std::string name) {

    FQuat quat = _value;
    float x, y, z, w;
    x = -quat.X;
    y = quat.Z;
    z = quat.Y;
    w = quat.W;

    std::memcpy(data, &x, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &y, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &z, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &w, sizeof(float));
}


template<>
void Parameter<FColor>::Serialize(char* data, std::string name) {

    FColor color = _value;
    float r, g, b, a;
    r = color.R;
    g = color.G;
    b = color.B;
    a = color.A;
    std::memcpy(data, &r, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &g, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &b, sizeof(float)); data += sizeof(float);
    std::memcpy(data, &a, sizeof(float));
}

template<>
void Parameter<std::string>::Serialize(char* data, std::string name) {

    std::string str = _value;
    std::memcpy(data, &str, str.size());
}

template<>
int Parameter<std::string>::dataSize() {

        int length = _value.length();
        return length;

}



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