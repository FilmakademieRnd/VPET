// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#include "Parameter.h"

#include "CoreMinimal.h"

#include "Components/ActorComponent.h"

#include "ParameterObject.generated.h"

//class AbstractParameter;
typedef TMulticastDelegate<void(AbstractParameter*)> FVpet_ParameterObject_Delegate;

//DECLARE_DELEGATE_OneParam(FParameterModifiedSignature, AbstractParameter);
//DECLARE_DELEGATE_OneParam(FVpet_ParameterObject_Delegate, AbstractParameter, param);

UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class VPET_API UParameterObject : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UParameterObject();
	FVpet_ParameterObject_Delegate ParameterObject_HasChanged;

	int ID;
	
protected:
	// Called when the game starts
	 void BeginPlay() override;

public:	
	// Called every frame
	 void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

private:
	TArray<AbstractParameter*> _parameterList;

public:

	//FParameterModifiedSignature OnParameterModified;
	
	void AddParameter(AbstractParameter* param)
	{
		_parameterList.Add(param);
	}

	TArray<AbstractParameter*>* GetParameterList()
	{
		return &_parameterList;
	}

	void PrintParams();
		
};
