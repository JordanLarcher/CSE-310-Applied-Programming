// Global timer state management
let timerState = {
    mode: 'work',          // Current timer mode: 'work', 'short_break', or 'long_break'
    timeRemaining: 25 * 60, // Time remaining in seconds (default: 25 minutes)
    currentTaskId: null,   // ID of the currently active task
    pomodoroCount: 0,      // Number of completed pomodoros in current session
    isRunning: false,      // Flag indicating if timer is currently running
    interval: null         // Reference to the setInterval timer
};

// DOM element references for UI updates
const timerDisplay = document.getElementById('timer-display');
const statusDisplay = document.getElementById('status-display');
const currentTaskDisplay = document.getElementById('current-task-name');

/**
 * Get CSRF token from meta tag or cookie
 * @returns {string} CSRF token value
 */
function getCsrfToken() {
    const metaTag = document.querySelector('meta[name="csrf-token"]');
    if (metaTag) {
        return metaTag.getAttribute('content');
    }
    // Fallback to cookie method
    const cookies = document.cookie.split(';');
    for (let cookie of cookies) {
        const [name, value] = cookie.trim().split('=');
        if (name === 'Play-CSRF-Token') {
            return decodeURIComponent(value);
        }
    }
    return 'nocheck'; // Fallback for development
}

/**
 * Update task progress in UI without page reload
 * @param {string} taskId - Task ID
 * @param {object} data - Response data with updated progress
 */
function updateTaskProgress(taskId, data) {
    // Find the task row and update progress display
    const taskRows = document.querySelectorAll('tbody tr');
    taskRows.forEach(row => {
        const startButton = row.querySelector('.btn-start');
        if (startButton && startButton.getAttribute('onclick').includes(taskId)) {
            const progressCell = row.cells[1]; // Progress column
            const statusCell = row.cells[2];   // Status column
            
            // Update progress
            progressCell.textContent = `${data.completed}/${data.completed === row.dataset.estimated ? data.completed : row.dataset.estimated}`;
            
            // Update status if completed
            if (data.status === 'done') {
                statusCell.innerHTML = '<span class="badge-done">Completed</span>';
                startButton.style.display = 'none'; // Hide start button for completed tasks
            }
        }
    });
}

/**
 * Start the timer for a specific task
 * Called when user clicks the start button on a task
 * @param {string} taskId - Unique identifier of the task
 * @param {string} taskName - Display name of the task
 */
function startTimer(taskId, taskName) {
    // Prevent restarting if same task is already running
    if (timerState.isRunning && timerState.currentTaskId === taskId) return;

    // Reset to work mode if switching tasks
    if (timerState.currentTaskId !== taskId) {
        timerState.mode = 'work';
        timerState.timeRemaining = 25 * 60;
        timerState.currentTaskId = taskId;
        currentTaskDisplay.textContent = "Working on: " + taskName;
        currentTaskDisplay.style.display = 'block';
    }

    timerState.isRunning = true;

    // Clear existing interval and start new one
    if (timerState.interval) clearInterval(timerState.interval);
    timerState.interval = setInterval(tick, 1000);

    updateDisplay();
}

/**
 * Timer tick function called every second
 * Decrements time remaining and handles cycle completion
 */
function tick() {
    if (timerState.timeRemaining > 0) {
        timerState.timeRemaining--;
        updateDisplay();
    } else {
        finishCycle();
    }
}

/**
 * Update the timer display with current time and status
 * Formats time as MM:SS and updates status text and colors
 */
function updateDisplay() {
    const minutes = Math.floor(timerState.timeRemaining / 60);
    const seconds = timerState.timeRemaining % 60;
    timerDisplay.textContent = `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;

    // Update display colors and status based on current mode
    if (timerState.mode === 'work') {
        timerDisplay.style.color = '#d9534f'; // Red for work
        statusDisplay.textContent = "🔥 FOCUS";
    } else {
        timerDisplay.style.color = '#5cb85c'; // Green for break
        statusDisplay.textContent = "☕ BREAK";
    }
}

/**
 * Handle completion of a timer cycle (work or break)
 * Plays alert sound, saves progress, and switches modes
 */
function finishCycle() {
    clearInterval(timerState.interval);
    timerState.isRunning = false;

    // Play completion sound (ensure alert.mp3 exists in public/audios/)
    try {
        new Audio('/assets/audios/alert.mp3').play();
    } catch (e) { console.log("Audio file not found"); }

    // Handle work session completion
    if (timerState.mode === 'work') {
        fetch(`/tasks/${timerState.currentTaskId}/complete`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Csrf-Token': getCsrfToken()
            }
        }).then(response => {
            if (response.ok) {
                return response.json();
            }
            throw new Error('Failed to complete task');
        }).then(data => {
            console.log("Progress saved:", data);
            // Update UI without page reload
            updateTaskProgress(timerState.currentTaskId, data);
        }).catch(error => {
            console.error("Error saving progress:", error);
        });

        timerState.pomodoroCount++;

        // Determine break type: long break after 4 pomodoros, short break otherwise
        if (timerState.pomodoroCount % 4 === 0) {
            timerState.mode = 'long_break';
            timerState.timeRemaining = 15 * 60; // 15 minutes
        } else {
            timerState.mode = 'short_break';
            timerState.timeRemaining = 5 * 60;  // 5 minutes
        }
    } else {
        // End of break -> return to work
        timerState.mode = 'work';
        timerState.timeRemaining = 25 * 60;
    }

    updateDisplay();
    alert("Time's up! Next phase: " + (timerState.mode === 'work' ? "Work" : "Break"));
}

/**
 * Pause the timer manually
 * Clears the interval and updates the running state
 */
function pauseTimer() {
    clearInterval(timerState.interval);
    timerState.isRunning = false;
}