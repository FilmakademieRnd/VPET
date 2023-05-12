// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#include "Parameter.h"

#include "CoreMinimal.h"

#include "Components/ActorComponent.h"

#include "ParameterObject.generated.h"

class AbstractParameter;

UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class VPET_API UParameterObject : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UParameterObject();

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

public:	
	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

private:
	TArray<AbstractParameter*> _parameterList;

public:
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
