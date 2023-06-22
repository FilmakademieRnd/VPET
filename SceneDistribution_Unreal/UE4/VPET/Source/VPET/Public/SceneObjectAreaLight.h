// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once
#include "SceneObjectLight.h"
#include "SceneObjectAreaLight.generated.h"

/**
 * 
 */
UCLASS()
class VPET_API USceneObjectAreaLight : public USceneObjectLight
{
	GENERATED_BODY()

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;
};
