// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once
#include "SceneObjectLight.h"
#include "Engine/SpotLight.h"
#include "Components/SpotLightComponent.h"
#include "SceneObjectSpotLight.generated.h"

/**
 * 
 */
UCLASS()
class VPET_API USceneObjectSpotLight : public USceneObjectLight
{
	GENERATED_BODY()

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	ASpotLight* kSpotLgt;
	USpotLightComponent* spotLgtCmp;
	float rangeFactor = 0.005;
	float angleFactor = 2.0;

	// Light range buffer
	float rangeBuffer;
	// Light angle buffer
	float angleBuffer;

	void EncodeParameterRangeMessage();
	void EncodeParameterAngleMessage();
};
