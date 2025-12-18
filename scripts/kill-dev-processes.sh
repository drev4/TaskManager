#!/bin/bash
# Kill Development Processes Script for macOS/Linux
# This script stops all running development servers for TaskManager

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;36m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

echo ""
echo -e "${YELLOW}🛑 Stopping TaskManager development processes...${NC}"
echo ""

# Function to kill processes by name
kill_process() {
    local process_name=$1
    local display_name=$2

    echo -e "${BLUE}  Stopping ${display_name}...${NC}"

    # Find processes
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        pids=$(pgrep -f "$process_name" 2>/dev/null)
    else
        # Linux
        pids=$(pgrep -f "$process_name" 2>/dev/null)
    fi

    if [ -z "$pids" ]; then
        echo -e "${GRAY}    ℹ No ${display_name} processes running${NC}"
    else
        for pid in $pids; do
            kill -9 "$pid" 2>/dev/null
            if [ $? -eq 0 ]; then
                echo -e "${GREEN}    ✓ Stopped PID ${pid}${NC}"
            else
                echo -e "${RED}    ✗ Failed to stop PID ${pid}${NC}"
            fi
        done
    fi
}

# Stop Node.js (Frontend)
kill_process "node.*vite" "Node.js (Frontend Vite)"

# Stop additional Node processes that might be related
node_pids=$(pgrep -x node 2>/dev/null)
if [ ! -z "$node_pids" ]; then
    echo -e "${BLUE}  Stopping remaining Node.js processes...${NC}"
    for pid in $node_pids; do
        # Check if it's related to our project
        if ps -p $pid -o command= | grep -q "TaskManager\|vite\|vue"; then
            kill -9 "$pid" 2>/dev/null
            echo -e "${GREEN}    ✓ Stopped Node.js PID ${pid}${NC}"
        fi
    done
fi

# Stop .NET processes (Backend)
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    dotnet_pids=$(pgrep -f "dotnet.*TaskMgr" 2>/dev/null)
else
    # Linux
    dotnet_pids=$(pgrep -f "dotnet.*TaskMgr" 2>/dev/null)
fi

if [ -z "$dotnet_pids" ]; then
    echo -e "${GRAY}    ℹ No .NET Backend processes running${NC}"
else
    echo -e "${BLUE}  Stopping .NET Backend...${NC}"
    for pid in $dotnet_pids; do
        kill -9 "$pid" 2>/dev/null
        echo -e "${GREEN}    ✓ Stopped .NET PID ${pid}${NC}"
    done
fi

echo ""
echo -e "${YELLOW}📋 Checking ports...${NC}"

# Check if ports are free
ports=(65454 3000 3001 5173)
ports_in_use=()

for port in "${ports[@]}"; do
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
        pid=$(lsof -Pi :$port -sTCP:LISTEN -t)
        ports_in_use+=($port)
        echo -e "${YELLOW}  ⚠ Port ${port} still in use by PID ${pid}${NC}"
    fi
done

if [ ${#ports_in_use[@]} -eq 0 ]; then
    echo -e "${GREEN}  ✓ All development ports are free${NC}"
else
    echo ""
    echo -e "${GRAY}  ℹ If you need to free these ports, kill the processes manually:${NC}"
    for port in "${ports_in_use[@]}"; do
        pid=$(lsof -Pi :$port -sTCP:LISTEN -t 2>/dev/null)
        if [ ! -z "$pid" ]; then
            echo -e "${GRAY}    kill -9 ${pid}${NC}"
        fi
    done
fi

echo ""
echo -e "${GREEN}✅ Done!${NC}"
echo ""
