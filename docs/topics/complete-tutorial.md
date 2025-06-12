# Complete Tutorial: Building a Task Management App

This tutorial will walk you through building a complete task management application with Coalesce, demonstrating key concepts and patterns along the way.

## What We'll Build

By the end of this tutorial, you'll have a task management application with:
- User authentication and authorization
- Projects and tasks with assignments
- File attachments
- Activity tracking and notifications
- Admin dashboard

## Prerequisites

- .NET 8 SDK
- Node.js 18+ 
- Visual Studio or VS Code
- SQL Server (LocalDB is fine)

## Step 1: Project Setup

Create a new Coalesce project:

```bash
dotnet new install IntelliTect.Coalesce.Vue.Template
dotnet new coalescevue -n TaskManager -o TaskManager --include-auth --include-audit
cd TaskManager/*.Web
npm ci
dotnet restore
```

## Step 2: Design the Data Model

Let's start by defining our core entities in the Data project:

### User Entity

```csharp
// TaskManager.Data/Models/ApplicationUser.cs
public class ApplicationUser : IdentityUser
{
    [Display(Name = "Full Name")]
    [Search, ListText]
    public string FullName { get; set; } = "";

    [Display(Name = "Job Title")]
    public string? JobTitle { get; set; }

    [InternalUse]
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];

    [InternalUse]
    public ICollection<Task> AssignedTasks { get; set; } = [];

    [InternalUse]
    public ICollection<ActivityLog> Activities { get; set; } = [];
}
```

### Project Entity

```csharp
// TaskManager.Data/Models/Project.cs
[Read, Edit("Manager"), Create("Manager"), Delete("Manager")]
public class Project
{
    public int ProjectId { get; set; }

    [Required, Search, ListText]
    [Display(Name = "Project Name")]
    public string Name { get; set; } = "";

    [Display(Name = "Description")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [Display(Name = "Start Date")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "End Date")]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Status")]
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    // Navigation Properties
    public ICollection<ProjectMember> Members { get; set; } = [];
    public ICollection<Task> Tasks { get; set; } = [];

    // Computed Properties
    [NotMapped]
    public int TaskCount => Tasks?.Count ?? 0;

    [NotMapped]
    public int CompletedTaskCount => Tasks?.Count(t => t.Status == TaskStatus.Completed) ?? 0;

    [NotMapped]
    public decimal ProgressPercentage => TaskCount == 0 ? 0 : (decimal)CompletedTaskCount / TaskCount * 100;

    // Business Logic Methods
    [Coalesce, Execute("Manager")]
    public ItemResult AddMember(ApplicationUser user, ProjectRole role = ProjectRole.Member)
    {
        if (Members.Any(m => m.UserId == user.Id))
            return "User is already a member of this project";

        Members.Add(new ProjectMember
        {
            ProjectId = this.ProjectId,
            UserId = user.Id,
            Role = role,
            JoinedDate = DateTime.UtcNow
        });

        return "Member added successfully";
    }

    [Coalesce, Execute("Manager")]
    public ItemResult RemoveMember(string userId)
    {
        var member = Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            return "User is not a member of this project";

        Members.Remove(member);
        return "Member removed successfully";
    }
}

public enum ProjectStatus
{
    Planning,
    Active,
    OnHold,
    Completed,
    Cancelled
}

public enum ProjectRole
{
    Member,
    Manager,
    Owner
}
```

### Task Entity

```csharp
// TaskManager.Data/Models/Task.cs
[Read, Edit("ProjectMember"), Create("ProjectMember"), Delete("Manager")]
public class Task
{
    public int TaskId { get; set; }

    [Required, Search, ListText]
    [Display(Name = "Task Title")]
    public string Title { get; set; } = "";

    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [Required]
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [Display(Name = "Assigned To")]
    public string? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }

    [Display(Name = "Created By")]
    [Required]
    public string CreatedById { get; set; } = "";
    public ApplicationUser CreatedBy { get; set; } = null!;

    [Display(Name = "Priority")]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [Display(Name = "Status")]
    public TaskStatus Status { get; set; } = TaskStatus.Todo;

    [Display(Name = "Due Date")]
    public DateTime? DueDate { get; set; }

    [Display(Name = "Estimated Hours")]
    [Range(0, 1000)]
    public decimal? EstimatedHours { get; set; }

    [Display(Name = "Actual Hours")]
    [Range(0, 1000)]
    public decimal? ActualHours { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Completed Date")]
    public DateTime? CompletedDate { get; set; }

    // Navigation Properties
    public ICollection<TaskAttachment> Attachments { get; set; } = [];
    public ICollection<TaskComment> Comments { get; set; } = [];

    // Business Logic Methods
    [Coalesce, Execute("ProjectMember")]
    public async Task<ItemResult> MarkCompleted(AppDbContext db)
    {
        if (Status == TaskStatus.Completed)
            return "Task is already completed";

        Status = TaskStatus.Completed;
        CompletedDate = DateTime.UtcNow;

        // Log activity
        var activity = new ActivityLog
        {
            UserId = db.CurrentUserId,
            ProjectId = this.ProjectId,
            TaskId = this.TaskId,
            ActivityType = ActivityType.TaskCompleted,
            Description = $"Completed task: {this.Title}",
            Timestamp = DateTime.UtcNow
        };

        db.ActivityLogs.Add(activity);
        await db.SaveChangesAsync();

        return "Task marked as completed";
    }

    [Coalesce, Execute("ProjectMember")]
    public ItemResult AddComment(string content, AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "Comment content is required";

        var comment = new TaskComment
        {
            TaskId = this.TaskId,
            UserId = db.CurrentUserId,
            Content = content,
            CreatedDate = DateTime.UtcNow
        };

        Comments.Add(comment);
        return "Comment added successfully";
    }
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TaskStatus
{
    Todo,
    InProgress,
    InReview,
    Completed
}
```

### Supporting Entities

```csharp
// TaskManager.Data/Models/ProjectMember.cs
public class ProjectMember
{
    public int ProjectMemberId { get; set; }
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    
    public ProjectRole Role { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
}

// TaskManager.Data/Models/TaskAttachment.cs
[Read("ProjectMember"), Edit("TaskOwner"), Delete("TaskOwner")]
public class TaskAttachment
{
    public int TaskAttachmentId { get; set; }
    
    public int TaskId { get; set; }
    public Task Task { get; set; } = null!;
    
    [Required]
    public string FileName { get; set; } = "";
    
    [Required]
    public string FilePath { get; set; } = "";
    
    public long FileSize { get; set; }
    
    public string ContentType { get; set; } = "";
    
    public string UploadedById { get; set; } = "";
    public ApplicationUser UploadedBy { get; set; } = null!;
    
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
}

// TaskManager.Data/Models/TaskComment.cs
[Read("ProjectMember"), Edit("CommentOwner"), Delete("CommentOwner")]
public class TaskComment
{
    public int TaskCommentId { get; set; }
    
    public int TaskId { get; set; }
    public Task Task { get; set; } = null!;
    
    [Required]
    [DataType(DataType.MultilineText)]
    public string Content { get; set; } = "";
    
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? EditedDate { get; set; }
}

// TaskManager.Data/Models/ActivityLog.cs
[Read("ProjectMember"), InternalUse(InternalUseKind.CreateOnly | InternalUseKind.EditOnly)]
public class ActivityLog
{
    public int ActivityLogId { get; set; }
    
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public int? TaskId { get; set; }
    public Task? Task { get; set; }
    
    public ActivityType ActivityType { get; set; }
    public string Description { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum ActivityType
{
    ProjectCreated,
    ProjectUpdated,
    TaskCreated,
    TaskUpdated,
    TaskCompleted,
    TaskAssigned,
    CommentAdded,
    FileUploaded,
    MemberAdded,
    MemberRemoved
}
```

## Step 3: Configure Security

Create security attributes and data sources:

```csharp
// TaskManager.Data/Auth/SecurityAttributes.cs
public class ProjectMemberAttribute : SecurityAttribute
{
    public override string Name => "ProjectMember";
    
    public override string Description => "User must be a member of the related project";
}

public class TaskOwnerAttribute : SecurityAttribute
{
    public override string Name => "TaskOwner";
    
    public override string Description => "User must be the creator or assignee of the task";
}

public class CommentOwnerAttribute : SecurityAttribute
{
    public override string Name => "CommentOwner";
    
    public override string Description => "User must be the creator of the comment";
}
```

Create custom data sources for row-level security:

```csharp
// TaskManager.Data/DataSources/ProjectDataSource.cs
[Coalesce]
public class ProjectDataSource : StandardDataSource<Project, AppDbContext>
{
    public ProjectDataSource(CrudContext<AppDbContext> context) : base(context) { }

    public override IQueryable<Project> GetQuery(IDataSourceParameters parameters)
    {
        var userId = User.GetUserId();
        
        return Db.Projects
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .Where(p => p.Members.Any(m => m.UserId == userId) || User.IsInRole("Admin"));
    }
}

// TaskManager.Data/DataSources/TaskDataSource.cs
[Coalesce]
public class TaskDataSource : StandardDataSource<Task, AppDbContext>
{
    public TaskDataSource(CrudContext<AppDbContext> context) : base(context) { }

    public override IQueryable<Task> GetQuery(IDataSourceParameters parameters)
    {
        var userId = User.GetUserId();
        
        return Db.Tasks
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Where(t => t.Project.Members.Any(m => m.UserId == userId) || User.IsInRole("Admin"));
    }
}
```

## Step 4: Generate and Test

Generate the code and test the basic functionality:

```bash
dotnet coalesce
npm run build
dotnet run
```

Navigate to `/admin` to test the generated admin interface.

## Step 5: Create Custom Pages

### Project Dashboard

```vue
<!-- TaskManager.Web/src/views/project-dashboard.vue -->
<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <h1>{{ project.name }}</h1>
        <p>{{ project.description }}</p>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="8">
        <v-card>
          <v-card-title>Tasks</v-card-title>
          <v-card-text>
            <c-table 
              :list="taskList"
              :editable="false"
              :deletable="false"
              admin-route="/tasks"
            >
              <template #item.title="{ item }">
                <router-link :to="`/tasks/${item.taskId}`">
                  {{ item.title }}
                </router-link>
              </template>
              
              <template #item.assignedTo="{ item }">
                <v-chip v-if="item.assignedTo" color="primary" small>
                  {{ item.assignedTo.fullName }}
                </v-chip>
                <span v-else class="text--disabled">Unassigned</span>
              </template>
              
              <template #item.status="{ item }">
                <v-chip 
                  :color="getStatusColor(item.status)" 
                  small
                >
                  {{ item.status }}
                </v-chip>
              </template>
            </c-table>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card>
          <v-card-title>Project Progress</v-card-title>
          <v-card-text>
            <v-progress-circular
              :model-value="project.progressPercentage"
              :size="100"
              :width="15"
              color="primary"
            >
              {{ Math.round(project.progressPercentage) }}%
            </v-progress-circular>
            
            <div class="mt-4">
              <div>Total Tasks: {{ project.taskCount }}</div>
              <div>Completed: {{ project.completedTaskCount }}</div>
            </div>
          </v-card-text>
        </v-card>

        <v-card class="mt-4">
          <v-card-title>Team Members</v-card-title>
          <v-card-text>
            <v-list dense>
              <v-list-item 
                v-for="member in memberList.items"
                :key="member.projectMemberId"
              >
                <v-list-item-content>
                  <v-list-item-title>{{ member.user?.fullName }}</v-list-item-title>
                  <v-list-item-subtitle>{{ member.role }}</v-list-item-subtitle>
                </v-list-item-content>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ProjectViewModel, TaskListViewModel, ProjectMemberListViewModel } from '@/viewmodels.g'

const route = useRoute()
const projectId = computed(() => +route.params.id)

const project = new ProjectViewModel()
const taskList = new TaskListViewModel()
const memberList = new ProjectMemberListViewModel()

// Configure data sources
taskList.$params.filter = { projectId: projectId.value }
memberList.$params.filter = { projectId: projectId.value }

onMounted(async () => {
  await Promise.all([
    project.$load(projectId.value),
    taskList.$load(),
    memberList.$load()
  ])
})

function getStatusColor(status: string) {
  switch (status) {
    case 'Todo': return 'grey'
    case 'InProgress': return 'blue'
    case 'InReview': return 'orange'
    case 'Completed': return 'green'
    default: return 'grey'
  }
}
</script>
```

### Task Detail Page

```vue
<!-- TaskManager.Web/src/views/task-detail.vue -->
<template>
  <v-container>
    <v-row>
      <v-col cols="12" md="8">
        <v-card>
          <v-card-title>
            {{ task.title }}
            <v-spacer />
            <v-chip 
              :color="getStatusColor(task.status)" 
              class="ml-2"
            >
              {{ task.status }}
            </v-chip>
          </v-card-title>
          
          <v-card-text>
            <div class="mb-4">
              <h3>Description</h3>
              <p>{{ task.description || 'No description provided' }}</p>
            </div>

            <v-row>
              <v-col cols="6">
                <strong>Project:</strong> {{ task.project?.name }}
              </v-col>
              <v-col cols="6">
                <strong>Assigned To:</strong> {{ task.assignedTo?.fullName || 'Unassigned' }}
              </v-col>
              <v-col cols="6">
                <strong>Priority:</strong> {{ task.priority }}
              </v-col>
              <v-col cols="6">
                <strong>Due Date:</strong> {{ formatDate(task.dueDate) }}
              </v-col>
            </v-row>

            <div class="mt-4">
              <v-btn 
                v-if="task.status !== 'Completed'"
                color="success"
                @click="markCompleted"
                :loading="markCompletedLoading"
              >
                Mark Complete
              </v-btn>
            </div>
          </v-card-text>
        </v-card>

        <!-- Comments Section -->
        <v-card class="mt-4">
          <v-card-title>Comments</v-card-title>
          <v-card-text>
            <div v-for="comment in commentList.items" :key="comment.taskCommentId" class="mb-3">
              <div class="d-flex">
                <strong>{{ comment.user?.fullName }}</strong>
                <v-spacer />
                <small class="text--secondary">{{ formatDate(comment.createdDate) }}</small>
              </div>
              <p class="mt-1">{{ comment.content }}</p>
              <v-divider />
            </div>

            <!-- Add Comment Form -->
            <v-textarea
              v-model="newComment"
              label="Add a comment"
              rows="3"
              class="mt-4"
            />
            <v-btn 
              color="primary"
              @click="addComment"
              :loading="addCommentLoading"
              :disabled="!newComment.trim()"
            >
              Add Comment
            </v-btn>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <!-- Attachments -->
        <v-card>
          <v-card-title>Attachments</v-card-title>
          <v-card-text>
            <v-list dense>
              <v-list-item 
                v-for="attachment in attachmentList.items"
                :key="attachment.taskAttachmentId"
              >
                <v-list-item-content>
                  <v-list-item-title>{{ attachment.fileName }}</v-list-item-title>
                  <v-list-item-subtitle>
                    {{ formatFileSize(attachment.fileSize) }}
                  </v-list-item-subtitle>
                </v-list-item-content>
                <v-list-item-action>
                  <v-btn icon :href="attachment.filePath" target="_blank">
                    <v-icon>mdi-download</v-icon>
                  </v-btn>
                </v-list-item-action>
              </v-list-item>
            </v-list>

            <v-file-input
              v-model="selectedFile"
              label="Upload file"
              @change="uploadFile"
              class="mt-4"
            />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { TaskViewModel, TaskCommentListViewModel, TaskAttachmentListViewModel } from '@/viewmodels.g'

const route = useRoute()
const taskId = computed(() => +route.params.id)

const task = new TaskViewModel()
const commentList = new TaskCommentListViewModel()
const attachmentList = new TaskAttachmentListViewModel()

const newComment = ref('')
const selectedFile = ref<File[]>([])
const markCompletedLoading = ref(false)
const addCommentLoading = ref(false)

// Configure filters
commentList.$params.filter = { taskId: taskId.value }
attachmentList.$params.filter = { taskId: taskId.value }

onMounted(async () => {
  await Promise.all([
    task.$load(taskId.value),
    commentList.$load(),
    attachmentList.$load()
  ])
})

async function markCompleted() {
  markCompletedLoading.value = true
  try {
    const result = await task.markCompleted()
    if (result.wasSuccessful) {
      await task.$load(taskId.value) // Refresh
    }
  } finally {
    markCompletedLoading.value = false
  }
}

async function addComment() {
  if (!newComment.value.trim()) return
  
  addCommentLoading.value = true
  try {
    const result = await task.addComment(newComment.value)
    if (result.wasSuccessful) {
      newComment.value = ''
      await commentList.$load() // Refresh comments
    }
  } finally {
    addCommentLoading.value = false
  }
}

async function uploadFile() {
  // File upload implementation would go here
  // This would typically involve calling a file upload service
}

function getStatusColor(status: string) {
  switch (status) {
    case 'Todo': return 'grey'
    case 'InProgress': return 'blue'
    case 'InReview': return 'orange'
    case 'Completed': return 'green'
    default: return 'grey'
  }
}

function formatDate(date: string | null) {
  if (!date) return 'Not set'
  return new Date(date).toLocaleDateString()
}

function formatFileSize(bytes: number) {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}
</script>
```

## Step 6: Add File Upload Service

```csharp
// TaskManager.Data/Services/FileService.cs
[Coalesce]
public class FileService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public FileService(AppDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [Execute("ProjectMember")]
    public async Task<ItemResult<TaskAttachment>> UploadTaskAttachmentAsync(
        int taskId, 
        IFormFile file)
    {
        var task = await _db.Tasks
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);

        if (task == null)
            return "Task not found";

        // Check if user has access to this task
        var userId = _db.CurrentUserId;
        if (!task.Project.Members.Any(m => m.UserId == userId))
            return "Access denied";

        if (file.Length == 0)
            return "File is empty";

        if (file.Length > 10 * 1024 * 1024) // 10MB limit
            return "File size exceeds 10MB limit";

        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "tasks", taskId.ToString());
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new TaskAttachment
        {
            TaskId = taskId,
            FileName = file.FileName,
            FilePath = $"/uploads/tasks/{taskId}/{fileName}",
            FileSize = file.Length,
            ContentType = file.ContentType,
            UploadedById = userId,
            UploadedDate = DateTime.UtcNow
        };

        _db.TaskAttachments.Add(attachment);
        await _db.SaveChangesAsync();

        return attachment;
    }
}
```

## Step 7: Add Real-time Updates with SignalR

```csharp
// TaskManager.Web/Hubs/TaskHub.cs
public class TaskHub : Hub
{
    public async Task JoinProject(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Project_{projectId}");
    }

    public async Task LeaveProject(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Project_{projectId}");
    }
}

// TaskManager.Data/Services/NotificationService.cs
[Coalesce]
public class NotificationService
{
    private readonly IHubContext<TaskHub> _hubContext;

    public NotificationService(IHubContext<TaskHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyTaskUpdated(int projectId, int taskId, string message)
    {
        await _hubContext.Clients.Group($"Project_{projectId}")
            .SendAsync("TaskUpdated", new { taskId, message });
    }
}
```

## Step 8: Testing and Deployment

Add unit tests:

```csharp
// TaskManager.Test/ProjectTests.cs
[TestClass]
public class ProjectTests
{
    [TestMethod]
    public void AddMember_NewUser_Success()
    {
        // Arrange
        var project = new Project { ProjectId = 1, Name = "Test Project" };
        var user = new ApplicationUser { Id = "user1", FullName = "Test User" };

        // Act
        var result = project.AddMember(user, ProjectRole.Member);

        // Assert
        Assert.IsTrue(result.WasSuccessful);
        Assert.AreEqual(1, project.Members.Count);
    }

    [TestMethod]
    public void AddMember_ExistingUser_Fails()
    {
        // Arrange
        var project = new Project { ProjectId = 1, Name = "Test Project" };
        var user = new ApplicationUser { Id = "user1", FullName = "Test User" };
        project.Members.Add(new ProjectMember { UserId = user.Id, Role = ProjectRole.Member });

        // Act
        var result = project.AddMember(user, ProjectRole.Member);

        // Assert
        Assert.IsFalse(result.WasSuccessful);
        Assert.AreEqual("User is already a member of this project", result.Message);
    }
}
```

## Conclusion

You now have a complete task management application that demonstrates:

- **Entity modeling** with proper relationships and business logic
- **Security** with row-level access control and role-based permissions  
- **Custom data sources** for filtering data based on user context
- **Custom methods** for business operations
- **File handling** with proper validation and storage
- **Real-time updates** using SignalR
- **Custom Vue pages** that leverage generated ViewModels and components

This tutorial shows how Coalesce handles the complex plumbing while letting you focus on business logic and user experience. The generated APIs, TypeScript models, and admin interface provide a solid foundation that you can customize and extend as needed.

## Next Steps

- Add email notifications using the integration patterns
- Implement audit logging to track all changes
- Add reporting and analytics features
- Deploy to Azure or your preferred cloud platform
- Implement mobile-responsive design improvements
- Add advanced features like time tracking and project templates