// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "UpdateSenderThread.h"

// Update sender thread
void UpdateSenderThread::DoWork()
{
	DOL(doLog, Warning, "[VPET2 SEND Thread] zeroMQ update sender thread running");

	/* Retrieve high water mark into sndhwm */
	int type;
	size_t type_size = sizeof(type);

	while (1)
	{
		// Try something using the socket just to stop the thread
		try {
			socket->getsockopt(ZMQ_TYPE, &type, &type_size);
		}
		catch (const zmq::error_t& e)
		{
			FString errName = FString(zmq_strerror(e.num()));
			DOL(doLog, Error, "[SEND Thread] socket exception: %s", *errName);
			return;
		}

		//// Process messages
		//int count = 0;
		//for (size_t i = 0; i < msgQ->size(); i++)
		//{
		//	std::vector<uint8_t> msg = msgQ->at(i);

		//	
		//	// trying to send a message
		//	char* responseMessageContent = NULL;
		//	char* messageStart = NULL;
		//	int responseLength = 0;
		//	
		//	responseLength = msg.size();
		//	messageStart = responseMessageContent = (char*)malloc(responseLength);

		//	// iterate over bytes
		//	for (size_t j = 0; j < msg.size(); j++)
		//	{
		//		DOL(doLog, Error, "Message %i: %i", j, msg[j]);

		//		uint8_t intVal = msg[j];
		//		memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
		//		responseMessageContent += sizeof(uint8_t);

		//	}
		//	count++;

		//	
		//	// Send message
		//	DOL(doLog, Log, "[SEND Thread] Send message length: %d", responseLength);
		//	zmq::message_t responseMessage((void*)messageStart, responseLength, NULL);
		//	try {
		//		socket->send(responseMessage);
		//	}
		//	catch (const zmq::error_t& e)
		//	{
		//		FString errName = FString(zmq_strerror(e.num()));
		//		DOL(doLog, Error, "[SEND Thread] send exception: %s", *errName);
		//		return;
		//	}
		//	/*

		//	// 8 header + 4 for vector?
		//	responseLength = 5 + sizeof(bool);
		//	messageStart = responseMessageContent = (char*)malloc(responseLength);
		//	// CID
		//	//auto test = TCHAR_TO_ANSI(*FString("1"));
		//	//uint8_t intVal = *TCHAR_TO_ANSI(*FString("1"));
		//	//char chID = 0x7f;//0x01;
		//	uint8_t intVal = cID + 1;
		//	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
		//	responseMessageContent += sizeof(uint8_t);
		//	// Time
		//	intVal = 50;
		//	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
		//	responseMessageContent += sizeof(uint8_t);
		//	// Message type - lock 1 (param update = 0)
		//	intVal = 1;
		//	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
		//	responseMessageContent += sizeof(uint8_t);
		//	// Scene Obj ID
		//	uint16_t shortVal = 7;
		//	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
		//	responseMessageContent += sizeof(uint16_t);
		//	//// Parameter ID
		//	//shortVal = 3;
		//	//memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
		//	//responseMessageContent += sizeof(uint16_t);
		//	//// Parameter type
		//	//intVal = 3;
		//	//memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
		//	//responseMessageContent += sizeof(uint8_t);
		//	//// Actual message
		//	//// Maybe a float?
		//	//float msgVal = 40;
		//	//memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
		//	//responseMessageContent += sizeof(float);
		//	bool lockVal = true;
		//	memcpy(responseMessageContent, (char*)&lockVal, sizeof(bool));
		//	responseMessageContent += sizeof(bool);


		//	// Send message
		//	DOL(doLog, Log, "[SEND Thread] Send message length: %d", responseLength);
		//	zmq::message_t responseMessage((void*)messageStart, responseLength, NULL);
		//	try {
		//		socket->send(responseMessage);
		//	}
		//	catch (const zmq::error_t& e)
		//	{
		//		FString errName = FString(zmq_strerror(e.num()));
		//		DOL(doLog, Error, "[SEND Thread] send exception: %s", *errName);
		//		return;
		//	}
		//	/**/
		//}

		//// Clean processed messages
		//msgQ->erase(msgQ->begin(), msgQ->begin() + count);

		// Process messages
		int count = 0;
		for (size_t i = 0; i < msgData->size(); i++)
		{
			count++;

			// Send message
			DOL(doLog, Log, "[SEND Thread] Send message length: %d", msgLen->at(i));
			zmq::message_t responseMessage((void*)msgData->at(i), msgLen->at(i), NULL);
			try {
				socket->send(responseMessage);
			}
			catch (const zmq::error_t& e)
			{
				FString errName = FString(zmq_strerror(e.num()));
				DOL(doLog, Error, "[SEND Thread] send exception: %s", *errName);
				return;
			}
		}

		// Clean processed messages
		msgData->erase(msgData->begin(), msgData->begin() + count);
		msgLen->erase(msgLen->begin(), msgLen->begin() + count);


		//DOL(doLog, Error, "Message true");

		// Safety halt
		// Keeps the thread from running wild
		Sleep(10);
	}
	/*
	while (1)
	{
		Sleep(5000);

		DOL(doLog, Error, "[VPET2 SEND Thread] TEST 5 secs");

		//
		DOL(doLog, Error, "[VPET2 SEND Thread] CID %d", cID);

		// trying to send any message

		char* responseMessageContent = NULL;
		char* messageStart = NULL;
		int responseLength = 0;


		// 7 header + 4 for vector?
		responseLength = 8 + sizeof(float);
		messageStart = responseMessageContent = (char*)malloc(responseLength);
		// CID
		//auto test = TCHAR_TO_ANSI(*FString("1"));
		//uint8_t intVal = *TCHAR_TO_ANSI(*FString("1"));
		//char chID = 0x7f;//0x01;
		uint8_t intVal = cID;
		memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
		responseMessageContent += sizeof(uint8_t);
		// Time
		intVal = 50;
		memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
		responseMessageContent += sizeof(uint8_t);
		// Param
		intVal = 0;
		memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
		responseMessageContent += sizeof(uint8_t);
		// Scene Obj ID
		uint16_t shortVal = 6;
		memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
		responseMessageContent += sizeof(uint16_t);
		// Parameter ID
		shortVal = 3;
		memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
		responseMessageContent += sizeof(uint16_t);
		// Parameter type
		intVal = 3;
		memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
		responseMessageContent += sizeof(uint8_t);
		// Actual message
		// Maybe a float?
		float msgVal = 40;
		memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
		responseMessageContent += sizeof(float);


		// Send message
		DOL(doLog, Log, "[SEND Thread] Send message length: %d", responseLength);
		zmq::message_t responseMessage((void*)messageStart, responseLength, NULL);
		try {
			socket->send(responseMessage);
		}
		catch (const zmq::error_t& e)
		{
			FString errName = FString(zmq_strerror(e.num()));
			DOL(doLog, Error, "[SEND Thread] send exception: %s", *errName);
			return;
		}

	}
	/*
	while (1)
	{
		char* responseMessageContent = NULL;
		char* messageStart = NULL;
		int responseLength = 0;

		// Blocking receive
		try {
			socket->recv(&message);
		}
		catch (const zmq::error_t& e)
		{
			FString errName = FString(zmq_strerror(e.num()));
			DOL(doLog, Error, "[SYNC Thread] recv exception: %s", *errName);
			return;
		}

		const char* msgPointer = static_cast<const char*>(message.data());
		if (msgPointer == NULL) {
			DOL(doLog, Error, "[SYNC Thread] Error msgPointer is NULL");
		}
		else
		{
			// Shifting into std::vector
			byteVector.clear();
			byteStream = static_cast<uint8_t*>(message.data()), message.size();
			for (size_t i = 0; i < message.size(); i++)
			{
				byteVector.push_back(byteStream[i]);
			}

			// Process message
			// Byte zero -> cID
			// Ignore message from host
			if (byteVector[0] != cID)
			{
				// Byte 1 -> time
				// Byte 2 -> Parameter update
				switch ((MessageType)byteVector[2])
				{
				case MessageType::LOCK:
					//decodeLockMessage(ref input);
					UE_LOG(LogTemp, Log, TEXT("[VPET2 Parse] Lock message"));
					break;
				case MessageType::SYNC:
					//if (!core.isServer)
					//	decodeSyncMessage(ref input);
					UE_LOG(LogTemp, Log, TEXT("[VPET2 Parse] Sync message"));
					break;
				case MessageType::UNDOREDOADD:
					//decodeUndoRedoMessage(ref input);
					UE_LOG(LogTemp, Log, TEXT("[VPET2 Parse] Undo/Redo message"));
					break;
				case MessageType::RESETOBJECT:
					//decodeResetMessage(ref input);
					UE_LOG(LogTemp, Log, TEXT("[VPET2 Parse] Reset message"));
					break;
				case MessageType::PARAMETERUPDATE:
					// input[1] is time
					//m_messageBuffer[input[1]].Add(input);
					UE_LOG(LogTemp, Log, TEXT("[VPET2 Parse] Parameter updated message"));
					msgQ->push_back(byteVector);
					break;
				default:
					break;
				}
			}
		}

		// Fallback for free-running while
		//Sleep(10);

	}
	/**/
}