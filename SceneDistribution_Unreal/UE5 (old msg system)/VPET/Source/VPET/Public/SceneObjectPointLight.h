// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once
#include "SceneObjectLight.h"
#include "Engine/PointLight.h"
#include "Components/PointLightComponent.h"
#include "SceneObjectPointLight.generated.h"

/**
 * 
 */
UCLASS()
class VPET_API USceneObjectPointLight : public USceneObjectLight
{
	GENERATED_BODY()

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	APointLight* kPointLgt;
	UPointLightComponent* pointLgtCmp;
	float rangeFactor = 0.005;

	// Light range buffer
	Parameter<float>* Range_Vpet_Param;

	void EncodeParameterRangeMessage();
};
