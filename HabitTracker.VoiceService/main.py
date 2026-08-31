import datetime
from typing import Optional
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from langchain_ollama import ChatOllama
from langchain_core.prompts import ChatPromptTemplate

app = FastAPI(
    title="HabitTracker Voice Extraction Service",
    description="Extracts structured habit information from voice transcriptions using LangChain and local Ollama model qwen2.5:3b."
)

# ==========================================
# CONFIGURATION
# ==========================================
# Set the allowed CORS origins to match your frontend Web application URL.
# By default, HabitTracker.Web runs on https://localhost:7123 and http://localhost:5123.
ALLOWED_ORIGINS = [
    "https://localhost:7123",
    "http://localhost:5123",
]

# Set the Ollama endpoint. Default local Ollama URL is http://localhost:11434.
OLLAMA_BASE_URL = "http://localhost:11434"
OLLAMA_MODEL = "qwen2.5:3b"
# ==========================================

app.add_middleware(
    CORSMiddleware,
    allow_origins=ALLOWED_ORIGINS,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class TranscriptRequest(BaseModel):
    transcript: str

class HabitExtraction(BaseModel):
    Title: Optional[str] = Field(None, description="The title or name of the habit (e.g. 'Drink Water', 'Morning Run', 'Study SQL').")
    Description: Optional[str] = Field(None, description="A detailed description or instructions for the habit.")
    Frequency: Optional[int] = Field(None, description="The frequency of the habit. Use: 0 for Daily, 1 for Weekly, 2 for Monthly. E.g., 'every day' or 'daily' maps to 0. 'once a week' maps to 1.")
    Status: Optional[int] = Field(0, description="The status of the habit. Defaults to 0 (Active).")
    KanbanStatus: Optional[int] = Field(0, description="The Kanban status of the habit. Defaults to 0 (Todo).")
    StartDate: Optional[str] = Field(None, description="The start date of the habit in YYYY-MM-DD format.")
    EndDate: Optional[str] = Field(None, description="The end date of the habit in YYYY-MM-DD format. Set to null if not mentioned or cannot be inferred from duration or dates in the transcript.")

# Initialize ChatOllama with structured output
try:
    llm = ChatOllama(
        model=OLLAMA_MODEL,
        base_url=OLLAMA_BASE_URL,
        temperature=0.0
    )
    structured_llm = llm.with_structured_output(HabitExtraction)
except Exception as e:
    # We will log the error but allow startup; the endpoint will fail gracefully if Ollama is not running.
    print(f"Error initializing LangChain Ollama client: {e}")
    structured_llm = None

@app.post("/extract-habit", response_model=HabitExtraction)
async def extract_habit(request: TranscriptRequest):
    if not request.transcript or not request.transcript.strip():
        raise HTTPException(status_code=400, detail="Transcript cannot be empty.")

    if structured_llm is None:
        raise HTTPException(
            status_code=503, 
            detail="Ollama connection is not initialized. Please ensure Ollama is running locally and qwen2.5:3b model is pulled."
        )

    today = datetime.date.today()
    today_str = today.strftime("%Y-%m-%d")

    system_prompt = (
        "You are an AI assistant designed to extract structured habit information from a spoken transcript. "
        "The current date (today) is {today_str}.\n"
        "Analyze the transcript and fill in the structured fields. "
        "For relative dates (like 'starting tomorrow', 'ending next week', 'for two weeks'), compute the exact date relative to today's date ({today_str}).\n"
        "If a specific field cannot be confidently extracted from the text, return null (None) for that field."
    )

    prompt = ChatPromptTemplate.from_messages([
        ("system", system_prompt),
        ("human", "Extract from this transcript: {transcript}")
    ])

    try:
        # Format the prompt
        formatted_prompt = prompt.format_messages(today_str=today_str, transcript=request.transcript)
        
        # Invoke the structured LLM
        result = structured_llm.invoke(formatted_prompt)
        
        # Handle fallback if LLM returns a dictionary instead of Pydantic model
        if isinstance(result, dict):
            extracted = HabitExtraction(**result)
        else:
            extracted = result

        # Fallback date logic:
        # If the start date is empty, default to today
        if not extracted.StartDate:
            extracted.StartDate = today_str

        # If the user doesn't mention an end date / duration end / deadline in their speech,
        # default the end date to 21 days from the current date (today).
        if not extracted.EndDate:
            extracted.EndDate = (today + datetime.timedelta(days=21)).strftime("%Y-%m-%d")

        return extracted

    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"An error occurred during LLM habit extraction: {str(e)}"
        )

@app.get("/")
def read_root():
    return {
        "status": "Online",
        "service": "HabitTracker.VoiceService",
        "ollama_model": OLLAMA_MODEL,
        "ollama_url": OLLAMA_BASE_URL
    }
