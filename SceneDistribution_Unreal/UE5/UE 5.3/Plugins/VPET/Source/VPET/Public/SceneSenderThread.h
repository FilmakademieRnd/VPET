// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include <zmq.hpp>
#include "Async/AsyncWork.h"
#include "SceneDistributorState.h"

// Development output log macro
#define DOL(logType, logVerbosity, logString, ...) \
if (logType) { \
UE_LOG(LogTemp, logVerbosity, TEXT(logString), ##__VA_ARGS__); \
}

// Development on-screen debug message
#define OSD(debugColor, debugString, ...) if(GEngine && VerboseDisplay) GEngine->AddOnScreenDebugMessage(-1, 5.0f, debugColor, FString::Printf(TEXT(debugString), __VA_ARGS__));


// Distribution thread
class SceneSenderThread : public FNonAbandonableTask
{
	friend class FAutoDeleteAsyncTask<SceneSenderThread>;
public:
	zmq::socket_t* socket;
	VPET::SceneDistributorState* m_sharedState;
	bool doLog;

	SceneSenderThread(zmq::socket_t* pSocket, VPET::SceneDistributorState* pState, bool pLog) : socket(pSocket), m_sharedState(pState), doLog(pLog) { }

	void DoWork();

	FORCEINLINE TStatId GetStatId() const
	{
		RETURN_QUICK_DECLARE_CYCLE_STAT(FGenericTask, STATGROUP_TaskGraphTasks);
	}

};