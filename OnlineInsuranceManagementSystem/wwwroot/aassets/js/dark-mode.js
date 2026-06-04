  document.addEventListener("DOMContentLoaded", () => {
    const html = document.documentElement;
    const body = document.body;
    const themeToggle = document.getElementById("themeToggle");
    const themeIcon = document.getElementById("themeIcon");

    // Load saved theme or default to light
    const savedTheme = localStorage.getItem("theme") || "light";
    applyTheme(savedTheme);

    // Toggle theme on click
    themeToggle.addEventListener("click", () => {
      const currentTheme = html.getAttribute("data-bs-theme");
      const newTheme = currentTheme === "light" ? "dark" : "light";
      applyTheme(newTheme);
      localStorage.setItem("theme", newTheme);
    });

    function applyTheme(theme) {
      // Set Bootstrap theme attribute
      html.setAttribute("data-bs-theme", theme);

      // Set or remove dark-mode class for custom CSS
      if (theme === "dark") {
        body.classList.add("dark-mode");
        themeIcon.src = "../assets/img/favicon/sun.ico"; // show sun in dark mode
      } else {
        body.classList.remove("dark-mode");
        themeIcon.src = "../assets/img/favicon/moon.ico"; // show moon in light mode
      }
    }
  });

document.addEventListener("DOMContentLoaded", function () {
  const calendarEl = document.getElementById("calendar");

  const calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: "dayGridMonth",
    headerToolbar: {
      left: "prev,next today",
      right: "title",
    },
    themeSystem: "bootstrap5",
    height: 400,
    selectable: true,
    editable: true,
    select: function (info) {
      // Open modal when a date is selected
      const todoModal = new bootstrap.Modal(document.getElementById("todoModal"));
      document.getElementById("todoDate").value = info.startStr;
      document.getElementById("todoForm").reset();
      todoModal.show();
    },
    events: [],
  });

  calendar.render();

  const todoList = document.getElementById("todoList");

  document.getElementById("saveTodo").addEventListener("click", function () {
    const todoText = document.getElementById("todoText").value;
    const todoTime = document.getElementById("todoTime").value;
    const todoDate = document.getElementById("todoDate").value;
    const todoDaily = document.getElementById("todoDaily").checked;

    if (!todoText || !todoTime) return;

    function addTodoItem(date, text, time, daily) {
      const li = document.createElement("li");
      li.className = "list-group-item d-flex justify-content-between align-items-center";
      li.innerHTML = `
        <div>
          <strong>${text}</strong><br>
          <small>${date} ${time}${daily ? " (Daily)" : ""}</small>
        </div>
        <button class="btn btn-sm btn-danger">Delete</button>
      `;
      li.querySelector("button").addEventListener("click", () => li.remove());
      todoList.appendChild(li);
    }

    // Add the task to the list
    addTodoItem(todoDate, todoText, todoTime, todoDaily);

    // Optionally, add recurring tasks for the next 30 days
    if (todoDaily) {
      for (let i = 1; i <= 30; i++) {
        const nextDate = new Date(todoDate);
        nextDate.setDate(nextDate.getDate() + i);
        const nextDateStr = nextDate.toISOString().split("T")[0];
        addTodoItem(nextDateStr, todoText, todoTime, true);
      }
    }

    bootstrap.Modal.getInstance(document.getElementById("todoModal")).hide();
  });
});


const brandLogo = document.getElementById('brand-logo');
const themeIcon = document.getElementById('themeIcon');

function updateLogo() {
  if (document.body.classList.contains('dark-mode')) {
    brandLogo.src = '../assets/img/favicon/dark-logo.svg';
  } else {
    brandLogo.src = '../assets/img/favicon/light-logo.svg';
  }
}

// Run on page load
updateLogo();

// Run whenever dark mode toggles
themeIcon.addEventListener('click', () => {
  document.body.classList.toggle('dark-mode');
  updateLogo();
});
