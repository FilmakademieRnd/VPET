// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#include <string>
#include <stdio.h>
#include <vector>
#include <typeindex>
#include <any>
#include <mutex>

#include "CoreMinimal.h"
#include "ParameterObject.h"

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

    // Base template will return UNKNOWN
    template <typename T>
    ParameterType toVPETType() {
        return ParameterType::UNKNOWN;
    }

    // Template specializations
    template <>
    ParameterType toVPETType<void>() {
        return ParameterType::NONE;
    }

    template <>
    ParameterType toVPETType<TFunction<void()>>() {
        return ParameterType::ACTION;
    }

    template <>
    ParameterType toVPETType<bool>() {
        return ParameterType::BOOL;
    }

    template <>
    ParameterType toVPETType<int32>() {
        return ParameterType::INT;
    }

    template <>
    ParameterType toVPETType<float>() {
        return ParameterType::FLOAT;
    }

    template <>
    ParameterType toVPETType<FVector2D>() {
        return ParameterType::VECTOR2;
    }

    template <>
    ParameterType toVPETType<FVector>() {
        return ParameterType::VECTOR3;
    }

    template <>
    ParameterType toVPETType<FVector4>() {
        return ParameterType::VECTOR4;
    }

    template <>
    ParameterType toVPETType<FQuat>() {
        return ParameterType::QUATERNION;
    }

    template <>
    ParameterType toVPETType<FColor>() {
        return ParameterType::COLOR;
    }

    template <>
    ParameterType toVPETType<FString>() {
        return ParameterType::STRING;
    }

    template <>
    ParameterType toVPETType<TArray<int32>>() {
        return ParameterType::LIST;
    }


    /*// unreat types to Vpet types
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
    */
    
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
        switch (_type) {
        case ParameterType::STRING:
            return static_cast<int>(std::any_cast<std::string>(_value).length());
        default:
            return _dataSize;
        }
    }
    
    //Parameter(T value, std::string name, void (*callback_func)(std::vector<uint8_t> kMsg), UParameterObject* parent = null, bool distribute = true)
    Parameter(T value, AActor* actor, std::string name, void (*callback_func)(std::vector<uint8_t> kMsg, AActor* actor), UParameterObject* parent, bool distribute = true)
    {
        _value = value;
        _name = name;
        _parent = parent;
        _actor = actor;
        _type = toVPETType<decltype(_value)>();
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
       
        switch (_type)
        {
        case ParameterType::BOOL:
            {
                bool val = std::any_cast<bool>(_value);
                std::memcpy(data, &val, sizeof(bool));
                break;
            }
        case ParameterType::INT:
            {
                int32 val = std::any_cast<int32>(_value);
                std::memcpy(data, &val, sizeof(int32));
                break;
            }
        case ParameterType::FLOAT:
            {
                float val = std::any_cast<float>(_value);
                std::memcpy(data, &val, sizeof(float));
                break;
            }
        case ParameterType::VECTOR2:
            {
                FVector2D vec2 = std::any_cast<FVector2D>(_value);
                float x,y;
                x = vec2.X;
                y = vec2.Y;
                std::memcpy(data, &x, sizeof(float)); data += sizeof(float);
                std::memcpy(data, &y, sizeof(float));
                break;
            }
        case ParameterType::VECTOR3:
            {
                FVector vec3 = std::any_cast<FVector>(_value);
                float x,y,z;
                x=vec3.X;
                y=vec3.Z;
                z=vec3.Y;
                //UE_LOG(LogTemp, Warning, TEXT("FVector values before memcpy inside param.h: %f %f %f"), vec3.X, vec3.Y, vec3.Z);
                if (name == "position")
                {
                    x=vec3.X * -0.01;
                    y=vec3.Z * 0.01;
                    z=vec3.Y * 0.01;   
                }
                
                std::memcpy(data, &x, sizeof(float)); data += sizeof(float); 
                std::memcpy(data, &y, sizeof(float)); data += sizeof(float);
                std::memcpy(data, &z, sizeof(float));
                
                break;
            }
        case ParameterType::VECTOR4:
            {
                FVector4 vec4 = std::any_cast<FVector4>(_value);
                float x,y,z,w;
                x = vec4.X;
                y = vec4.Y;
                z = vec4.Z;
                w = vec4.W;

                std::memcpy(data, &x, sizeof(float)); data += sizeof(float); 
                std::memcpy(data, &y, sizeof(float)); data += sizeof(float); 
                std::memcpy(data, &z, sizeof(float)); data += sizeof(float); 
                std::memcpy(data, &w, sizeof(float)); 

                break;
            }

        case ParameterType::QUATERNION:
            {
                FQuat quat = std::any_cast<FQuat>(_value);
                float x,y,z,w;
                x = -quat.X;
                y = quat.Z;
                z = quat.Y;
                w = quat.W;

                std::memcpy(data, &x, sizeof(float)); data += sizeof(float); 
                std::memcpy(data, &y, sizeof(float)); data += sizeof(float); 
                std::memcpy(data, &z, sizeof(float)); data += sizeof(float); 
                std::memcpy(data, &w, sizeof(float)); 
                
                break;
            }

        case ParameterType::COLOR:
            {
                FColor color = std::any_cast<FColor>(_value);
                float r,g,b,a;
                r = color.R;
                g = color.G;
                b = color.B;
                a = color.A;
                std::memcpy(data, &r, sizeof(float)); data += sizeof(float); 
                std::memcpy(data, &g, sizeof(float)); data += sizeof(float); 
                std::memcpy(data, &b, sizeof(float)); data += sizeof(float); 
                std::memcpy(data, &a, sizeof(float)); 
                break;
            }

        case ParameterType::STRING:
            {
                std::string str = std::any_cast<std::string>(_value);
                std::memcpy(data, &str, str.size()); 
                break;
            }
            
            default:
                break;
        }
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