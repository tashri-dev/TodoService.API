using Microsoft.EntityFrameworkCore;
using TodoService.API;
using TodoService.API.Dtos;
using TodoService.API.Models;
using TodoService.API.persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddTodoDbContext();
builder.Services.AddControllers();
//add open api 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TodoService.API", Version = "v1" });
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoService.API v1"));


//TODO :: move this to a separate file and use it
//TODO :: add SSO authentication and OAuth 2.0 authorization
//TODO :: add logging middleware to actions and exceptions
//TODO :: replace the InMemoryDatabase with mongo db

//FIX :: Add validation to the todo item
//FIX :: Add a service layer to handle the business logic

//TODO: Add bulk operations to the todo items

//create a todo item
app.MapPost("/todo", async (CreateTodoItem todoItemRequest, TodoDb db) =>
{
    //explicitly convert the CreateTodoItem to TodoItem
    var todoItem = (TodoItem)todoItemRequest;
    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();
    return Results.Created($"/todo/{todoItem.Id}", todoItem);
});

//get all todo items
app.MapGet("/todo", async (TodoDb db) =>
{
    return Results.Ok(await db.Todos.ToListAsync());
});

//get a todo item by id
app.MapGet("/todo/{id}", async (int id, TodoDb db) =>
{
    var todoItem = await db.Todos.FindAsync(id);
    if (todoItem is null)
    {
        return Results.NotFound();
    }
    var todoItemDetails = (TodoItemDetails)todoItem;
    return Results.Ok(todoItemDetails);
});

//update a todo item
app.MapPut("/todo/{id}", async (int id, UpdateTodoItem todoItemRequest, TodoDb db) =>
{
    var todoItem = await db.Todos.FindAsync(id);
    if (todoItem is null)
    {
        return Results.NotFound();
    }

    //explicitly convert the UpdateTodoItem to TodoItem
    var updatedTodoItem = (TodoItem)todoItemRequest;
    todoItem.Name = updatedTodoItem.Name;
    todoItem.IsComplete = updatedTodoItem.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

//delete a todo item
app.MapDelete("/todo/{id}", async (int id, TodoDb db) =>
{
    var todoItem = await db.Todos.FindAsync(id);
    if (todoItem is null)
    {
        return Results.NotFound();
    }

    db.Todos.Remove(todoItem);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDefaultEndpoints();


app.Run();
