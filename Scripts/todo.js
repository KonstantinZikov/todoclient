


(function () {
    // add new task button click handler
    $("#newCreate").click(function() {
        var isCompleted = $('#newCompleted')[0].checked;
        var name = $('#newName')[0].value;

        tasksManager.createTask(isCompleted, name)
            .then(tasksManager.loadTasks)
            .done(function(tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);
            });
    });

    // bind update task checkbox click handler
    $("#tasks > tbody").on('change', '.completed', function () {
        var tr = $(this).parent().parent();
        var taskId = tr.attr("data-id");
        var isCompleted = tr.find('.completed')[0].checked;
        var name = tr.find('.name').text();
        
        tasksManager.updateTask(taskId, isCompleted, name)
            .then(tasksManager.loadTasks)
            .done(function (tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);
            });
    });

    // bind delete button click for future rows
    $('#tasks > tbody').on('click', '.delete-button', function() {
        var taskId = $(this).parent().parent().attr("data-id");
        tasksManager.deleteTask(taskId)
            .then(tasksManager.loadTasks)
            .done(function(tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);
            });
    });

    // load all tasks on startup
    tasksManager.loadTasks()
        .done(function(tasks) {
            tasksManager.displayTasks("#tasks > tbody", tasks);
        });
});