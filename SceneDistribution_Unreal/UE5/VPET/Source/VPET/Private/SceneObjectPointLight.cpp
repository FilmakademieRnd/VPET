// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneObjectPointLight.h"

// Message parsing pre-declarations
void UpdatePointRange(std::vector<uint8_t> kMsg, AActor* actor);

// Called when the game starts
void USceneObjectPointLight::BeginPlay()
{
	Super::BeginPlay();

	kPointLgt = Cast<APointLight>(kLit);
	if (kPointLgt)
		pointLgtCmp = kPointLgt->PointLightComponent;
	if (pointLgtCmp)
	{
		float range = pointLgtCmp->AttenuationRadius * rangeFactor;
		Range_Vpet_Param = new Parameter<float>(range, thisActor, "range", &UpdatePointRange, this);
	}
}

// Using the update loop to check for local parameter changes
void USceneObjectPointLight::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (pointLgtCmp)
	{
		float range = pointLgtCmp->AttenuationRadius * rangeFactor;

		if (range != Range_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("RANGE CHANGE"));
			ParameterObject_HasChanged.Broadcast(Range_Vpet_Param);
			Range_Vpet_Param->setValue(range);
		}
	}
}

// Parses a message for range change
void UpdatePointRange(std::vector<uint8_t> kMsg, AActor* actor)
{
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Try Type RANGE"));
	APointLight* kPointLgt = Cast<APointLight>(actor);
	UPointLightComponent* pointLgtCmp = NULL;
	if (kPointLgt)
		pointLgtCmp = kPointLgt->PointLightComponent;
	if (pointLgtCmp)
	{
		float rangeFactor = 0.005;

		float lK = *reinterpret_cast<float*>(&kMsg[0]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type RANGE: %f"), lK);

		pointLgtCmp->AttenuationRadius = lK / rangeFactor;
	}
}
