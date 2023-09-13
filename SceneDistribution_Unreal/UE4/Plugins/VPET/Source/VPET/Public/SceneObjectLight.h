// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#include "CoreMinimal.h"
#include "SceneObject.h"

#include "Engine/Light.h"

#include "SceneObjectLight.generated.h"

/**
 * 
 */
UCLASS()
class VPET_API USceneObjectLight : public USceneObject
{
	GENERATED_BODY()

protected:
	// Called when the game starts
	virtual void BeginPlay() override;
	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	ALight* kLit;
	float lightFactor = 0.2;

	// Light color buffer
	Parameter<FVector4>* Col_Vpet_Param;
	// Light intensity buffer
	Parameter<float>* lit_Vpet_Param;

};
