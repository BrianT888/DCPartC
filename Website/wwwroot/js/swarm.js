document.addEventListener("DOMContentLoaded", async () => {
    loadSwarm();
    setInterval(countDown, 1000);//doing it like this because i wanted a timer to show
    //setInterval(loadSwarm, 60000);//60 secs = 1 min
});

async function loadSwarm() {
    try {
        console.log("starting fetch");
        const response = await fetch('/api/swarm/GetAllClientsInSwarm');
        if (!response) throw new Error('Failed to get client list');
        var clientList = await response.json();
        console.log("json'd response");

        const cbody = document.getElementById('swarmTable');
        cbody.innerHTML = '';
        if (clientList.length == 0) {
            cbody.innerHTML = `<tr><td colspan="3" style="text-align:center;color:#999;">No Clients</td></tr>`;
        } else {
            clientList.forEach(c => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${c.clientId}</td>
                    <td>${c.ip}</td>
                    <td>${c.port}</td>
                    <td>${c.jobsFinished}</td>
                    `;
                cbody.appendChild(row);
            });
        }
        
    } catch (error){
        console.log("error " + error)
    }

}

let countdownSecs = 60;

function countDown() {
    const timer = document.getElementById('countdown');

    if (countdownSecs > 0) {
        timer.textContent = `Refreshing Page in ${countdownSecs}`;
        countdownSecs--;
    } else {
        timer.textContent = `Refreshing Page`;
        loadSwarm();
        countdownSecs = 60;
    }
}