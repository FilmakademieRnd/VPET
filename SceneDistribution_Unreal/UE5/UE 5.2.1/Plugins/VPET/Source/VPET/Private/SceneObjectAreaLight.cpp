// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneObjectAreaLight.h"

// Called when the game starts
void USceneObjectAreaLight::BeginPlay()
{
	Super::BeginPlay();
}

// Using the update loop to check for local parameter changes
void USceneObjectAreaLight::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);
	
}
