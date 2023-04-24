// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneObjectSpotLight.h"

// Message parsing pre-declarations
void UpdateRange(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateAngle(std::vector<uint8_t> kMsg, AActor* actor);

// Called when the game starts
void USceneObjectSpotLight::BeginPlay()
{
	Super::BeginPlay();

	kSpotLgt = Cast<ASpotLight>(kLit);
	if (kSpotLgt)
		spotLgtCmp = kSpotLgt->SpotLightComponent;
	if (spotLgtCmp)
	{
		float range = spotLgtCmp->AttenuationRadius * rangeFactor;
		new Parameter<float>(range, thisActor, "range", &UpdateRange, this);
		rangeBuffer = range;

		float angle = spotLgtCmp->OuterConeAngle * angleFactor;
		new Parameter<float>(angle, thisActor, "spot angle", &UpdateAngle, this);
		angleBuffer = angle;
	}
}

// Using the update loop to check for local parameter changes
void USceneObjectSpotLight::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (spotLgtCmp)
	{
		float range = spotLgtCmp->AttenuationRadius * rangeFactor;
		float angle = spotLgtCmp->OuterConeAngle * angleFactor;

		if (range != rangeBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("RANGE CHANGE"));
			rangeBuffer = range;
			EncodeParameterRangeMessage();
		}
		if (angle != angleBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("ANGLE CHANGE"));
			angleBuffer = angle;
			EncodeParameterAngleMessage();
		}
	}
}

// Prepare a message for range change
void USceneObjectSpotLight::EncodeParameterRangeMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + float
	responseLength = 8 + sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - range
	shortVal = 5;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 3 for float
	intVal = 3;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = rangeBuffer;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Prepare a message for angle change
void USceneObjectSpotLight::EncodeParameterAngleMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + float
	responseLength = 8 + sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - angle
	shortVal = 6;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 3 for float
	intVal = 3;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = angleBuffer;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Parses a message for range change
void UpdateRange(std::vector<uint8_t> kMsg, AActor* actor)
{
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Try Type RANGE"));
	ASpotLight* kSpotLgt = Cast<ASpotLight>(actor);
	USpotLightComponent* spotLgtCmp = NULL;
	if(kSpotLgt)
		spotLgtCmp = kSpotLgt->SpotLightComponent;
	if (spotLgtCmp)
	{
		float rangeFactor = 0.005;

		float lK = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type RANGE: %f"), lK);

		spotLgtCmp->AttenuationRadius = lK / rangeFactor;
	}
}

// Parses a message for angle change
void UpdateAngle(std::vector<uint8_t> kMsg, AActor* actor)
{
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Try Type SPOT ANGLE"));
	ASpotLight* kSpotLgt = Cast<ASpotLight>(actor);
	USpotLightComponent* spotLgtCmp = NULL;
	if(kSpotLgt)
		spotLgtCmp = kSpotLgt->SpotLightComponent;
	if (spotLgtCmp)
	{
		float angleFactor = 2.0;

		float lK = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type SPOT ANGLE: %f"), lK);

		spotLgtCmp->OuterConeAngle = lK / angleFactor;
	}
}
