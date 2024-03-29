// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneObject.h"

// Message parsing pre-declarations
void UpdatePosition(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateRotation(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateScale(std::vector<uint8_t> kMsg, AActor* actor);


// Sets default values for this component's properties
USceneObject::USceneObject()
{
	// Set to be ticked every frame.
	PrimaryComponentTick.bCanEverTick = true;
}

// Called when the game starts
void USceneObject::BeginPlay()
{
	Super::BeginPlay();

	thisActor = GetOwner();
	FVector pos;
	FQuat rot;
	FVector sca;

	if (thisActor->GetAttachParentActor()!= nullptr)
	{
		FTransform localTransform = thisActor->GetRootComponent()->GetRelativeTransform();
		pos = localTransform.GetTranslation();
		Position_Vpet_Param = new Parameter(pos, thisActor, "position", &UpdatePosition, this);
		rot = localTransform.GetRotation();
		Rotation_Vpet_Param = new Parameter(rot, thisActor, "rotation", &UpdateRotation, this);
		sca = localTransform.GetScale3D();
		Scale_Vpet_Param = new Parameter(sca, thisActor, "scale", &UpdateScale, this);

	}
	else
	{
		pos = thisActor->GetActorLocation();
		Position_Vpet_Param = new Parameter(pos, thisActor, "position", &UpdatePosition, this);
		rot = thisActor->GetActorRotation().Quaternion();
		Rotation_Vpet_Param = new Parameter(rot, thisActor, "rotation", &UpdateRotation, this);
		sca = thisActor->GetActorScale3D();
		Scale_Vpet_Param = new Parameter(sca, thisActor, "scale", &UpdateScale, this);
	}
	
	
}

// Using the update loop to check for local parameter changes
void USceneObject::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (_lock)
		return;

	FVector pos;
	FQuat rot;
	FVector sca;

	if (thisActor->GetAttachParentActor() != nullptr)
	{
		// Get local position, rotation, and scale directly.
		FTransform localTransform = thisActor->GetRootComponent()->GetRelativeTransform();

		pos = localTransform.GetTranslation();
		rot = localTransform.GetRotation();
		sca = localTransform.GetScale3D();
	}
	else
	{
		pos = thisActor->GetActorLocation();
		rot = thisActor->GetActorRotation().Quaternion();
		sca = thisActor->GetActorScale3D();
	}


	
	if (pos != Position_Vpet_Param->getValue())
	{
		UE_LOG(LogTemp, Warning, TEXT("LOC CHANGE - pos: %s, Position_Vpet_Param: %s"), *pos.ToString(), *Position_Vpet_Param->getValue().ToString());
		ParameterObject_HasChanged.Broadcast(Position_Vpet_Param);
		Position_Vpet_Param->setValue(pos);
	}
	
	if (rot != Rotation_Vpet_Param->getValue())
	{
		UE_LOG(LogTemp, Warning, TEXT("ROT CHANGE - rot: %s, Rotation_Vpet_Param: %s"), *rot.ToString(), *Rotation_Vpet_Param->getValue().ToString());
		ParameterObject_HasChanged.Broadcast(Rotation_Vpet_Param);
		Rotation_Vpet_Param->setValue(rot);

	}
	
	if (sca != Scale_Vpet_Param->getValue())
	{
		UE_LOG(LogTemp, Warning, TEXT("SCA CHANGE - sca: %s, Scale_Vpet_Param: %s"), *sca.ToString(), *Scale_Vpet_Param->getValue().ToString());
		ParameterObject_HasChanged.Broadcast(Scale_Vpet_Param);
		Scale_Vpet_Param->setValue(sca);

	}
	
}

// Parses a message for position change
void UpdatePosition(std::vector<uint8_t> kMsg, AActor* actor)
{
	float lX = *reinterpret_cast<float*>(&kMsg[0]);
	float lY = *reinterpret_cast<float*>(&kMsg[4]);
	float lZ = *reinterpret_cast<float*>(&kMsg[8]);
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type POS: %f %f %f"), lX, lY, lZ);

	// Transform actor pos
	FVector aLoc(-lX, lZ, lY);

	aLoc *= 100.0;

	actor->SetActorRelativeLocation(aLoc);

}

// Parses a message for rotation change
void UpdateRotation(std::vector<uint8_t> kMsg, AActor* actor)
{
	float lX = *reinterpret_cast<float*>(&kMsg[0]);
	float lY = *reinterpret_cast<float*>(&kMsg[4]);
	float lZ = *reinterpret_cast<float*>(&kMsg[8]);
	float lW = *reinterpret_cast<float*>(&kMsg[12]);
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type ROT: %f %f %f %f"), lX, lY, lZ, lW);

	// Transform actor rot
	FQuat aRot(-lX, lZ, lY, lW);

	// In the case of cameras and lights and maybe some other stuff that needs tweak (maybe could be transmitted from scene distribution as a bool already - no double chek)
	FString className = actor->GetClass()->GetName();
	if (className.Find("Light") > -1 || className == "CameraActor")
	{
		//DOL(LogBasic, Log, "[SYNC Parse] Tweak rot!");
		FRotator tempRot(0, 90, 0);
		FQuat transRot = tempRot.Quaternion();
		aRot *= transRot;
	}

	actor->SetActorRelativeRotation(aRot);
}

// Parses a message for scale change
void UpdateScale(std::vector<uint8_t> kMsg, AActor* actor)
{
	float lX = *reinterpret_cast<float*>(&kMsg[0]);
	float lY = *reinterpret_cast<float*>(&kMsg[4]);
	float lZ = *reinterpret_cast<float*>(&kMsg[8]);
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type SCALE: %f %f %f"), lX, lY, lZ);

	// transform actor sca
	FVector aSca(lX, lZ, lY);

	actor->SetActorRelativeScale3D(aSca);
}

