# TeamsForge Schema Implementation Guide

## 📊 Table of Contents
1. [Schema Overview](#schema-overview)
2. [Key Relationships](#key-relationships)
3. [CRUD Implementation Checklists](#crud-implementation-checklists)
4. [Common Query Patterns](#common-query-patterns)
5. [Business Rules & Constraints](#business-rules--constraints)
6. [Performance Optimization Tips](#performance-optimization-tips)

---

## Schema Overview

### Core Domains
- **Identity & Auth**: Users, Roles, Permissions
- **Teams**: Team management, memberships, invitations
- **Projects**: Project organization, categories, tags
- **Tasks**: Task management, comments, history, dependencies
- **Audit & Tracking**: Activity logs, time entries
- **Integrations**: API keys, notification settings

### Entity Count: 18 tables + ASP.NET Identity tables

---

## Key Relationships

### User → Teams
```
User creates Team → Teams.CreatedByUserId
User joins Team → TeamUserRole (bridge table)
User invited to Team → TeamInvitation
```

### Team → Projects → Tasks
```
Team (1) ←→ (N) Projects
Project (1) ←→ (N) Tasks
Task (1) ←→ (N) Subtasks (self-referencing via ParentTaskId)
```

### Tagging System
```
Team (1) ←→ (N) Tags
Tag (N) ←→ (N) Tasks via TaskTag
Tag (N) ←→ (N) Projects via ProjectTag
```

### Task Audit Trail
```
Task (1) ←→ (N) TaskHistory (who changed what)
Task (1) ←→ (N) TaskComment (discussions)
Task (1) ←→ (N) TimeEntry (time tracking)
```

---

## CRUD Implementation Checklists

### 🔐 1. USER MANAGEMENT

#### Register User
```csharp
// POST /api/auth/register
✅ Create ApplicationUser
✅ Assign default SystemRole (SystemUser)
✅ Create default NotificationSettings
✅ Log ActivityLog (EntityType: User, Action: "UserRegistered")
✅ Send welcome email
```

#### Update User Profile
```csharp
// PUT /api/users/{id}
✅ Validate ownership or admin role
✅ Update user fields
✅ Log ActivityLog (Action: "ProfileUpdated")
✅ Update UpdatedAt timestamp
```

#### Soft Delete User
```csharp
// DELETE /api/users/{id}
✅ Set IsDeleted = true, DeletedAt = DateTime.UtcNow
✅ Log ActivityLog (Action: "UserDeleted")
⚠️ Note: User FKs use Restrict - handle orphaned records
```

---

### 👥 2. TEAM MANAGEMENT

#### Create Team
```csharp
// POST /api/teams
✅ Create Team with CreatedByUserId = currentUser.Id
✅ Create TeamUserRole (UserId, TeamId, Role: TeamOwner)
✅ Log ActivityLog (EntityType: Team, Action: "TeamCreated")
✅ Create default ProjectCategory (e.g., "General")
```

#### Invite User to Team
```csharp
// POST /api/teams/{teamId}/invitations
✅ Verify caller is TeamLead or TeamOwner
✅ Check if user already a member
✅ Generate unique Token (Guid.NewGuid())
✅ Set ExpiresAt (e.g., 7 days)
✅ Create TeamInvitation with Status: Pending
✅ Send invitation email with token
✅ Log ActivityLog (Action: "InvitationSent")
```

#### Accept Invitation
```csharp
// POST /api/invitations/accept
✅ Validate token exists and not expired
✅ Verify Status = Pending
✅ Create TeamUserRole with specified Role
✅ Update invitation: Status = Accepted, AcceptedAt = DateTime.UtcNow
✅ Log ActivityLog (Action: "InvitationAccepted")
✅ Create Notification for team creator
```

#### Remove Team Member
```csharp
// DELETE /api/teams/{teamId}/members/{userId}
✅ Verify caller is TeamLead/Owner
✅ Prevent removing last owner
✅ Delete TeamUserRole
✅ Unassign user from all team's project tasks
✅ Log ActivityLog (Action: "MemberRemoved")
```

#### Archive Team
```csharp
// PUT /api/teams/{teamId}/archive
✅ Set IsArchived = true, ArchivedAt = DateTime.UtcNow
✅ Optionally archive all projects
✅ Log ActivityLog (Action: "TeamArchived")
```

---

### 📁 3. PROJECT MANAGEMENT

#### Create Project
```csharp
// POST /api/teams/{teamId}/projects
✅ Verify user is team member
✅ Create Project with TeamId, CreatedById
✅ Set Status = Planning, Priority = Medium (defaults)
✅ Log TaskHistory (Action: "ProjectCreated")
✅ Create Notification for team members
```

#### Update Project Status
```csharp
// PUT /api/projects/{projectId}/status
✅ Validate new status is valid ProjectStatus enum
✅ If Status → Completed, set CompletedAt
✅ Update UpdatedAt
✅ Log ActivityLog (Action: "ProjectStatusChanged", Details: JSON)
✅ Notify assigned users
```

#### Assign Category
```csharp
// PUT /api/projects/{projectId}/category
✅ Verify category belongs to same team
✅ Update CategoryId
✅ Log ActivityLog (Action: "CategoryAssigned")
```

#### Tag Project
```csharp
// POST /api/projects/{projectId}/tags
✅ Verify tag belongs to same team
✅ Create ProjectTag (ProjectId, TagId)
✅ Handle duplicate gracefully (unique constraint)
✅ Log ActivityLog (Action: "ProjectTagged")
```

#### Archive Project
```csharp
// PUT /api/projects/{projectId}/archive
✅ Set IsArchived = true
✅ Update UpdatedAt
✅ Log ActivityLog (Action: "ProjectArchived")
```

---

### ✅ 4. TASK MANAGEMENT

#### Create Task
```csharp
// POST /api/projects/{projectId}/tasks
✅ Verify user is team member
✅ Create ProjectTask with ProjectId, ReporterId = currentUser.Id
✅ Set Status = Todo, Priority = Medium (defaults)
✅ Calculate OrderIndex (max + 1)
✅ Create TaskHistory (Action: "TaskCreated")
✅ If AssignedUserId provided, create notification
```

#### Assign Task
```csharp
// PUT /api/tasks/{taskId}/assign
✅ Verify assignee is team member
✅ Update AssignedUserId
✅ Create TaskHistory (Action: "TaskAssigned", OldValue, NewValue)
✅ Create Notification for assignee
✅ Update UpdatedAt
```

#### Update Task Status
```csharp
// PUT /api/tasks/{taskId}/status
✅ Validate status transition (business rules)
✅ Check dependencies: if blocked, prevent completion
✅ If Status → InProgress, set StartedAt (if null)
✅ If Status → Done, set CompletedAt
✅ Calculate ActualHours from TimeEntries
✅ Create TaskHistory (Action: "StatusChanged", Field: "Status")
✅ Notify reporter and assigned user
```

#### Create Subtask
```csharp
// POST /api/tasks/{parentTaskId}/subtasks
✅ Verify parent task exists
✅ Create task with ParentTaskId = parentTaskId
✅ Inherit ProjectId from parent
✅ Create TaskHistory (Action: "SubtaskCreated")
```

#### Add Task Dependency
```csharp
// POST /api/tasks/{taskId}/dependencies
✅ Validate TaskId ≠ DependsOnTaskId (CHK_NoSelfDependency)
✅ Verify both tasks in same project
✅ Check for circular dependencies (A → B → A)
✅ Create TaskDependency
✅ If DependencyType = Blocks, set IsBlocked on dependent task
✅ Create TaskHistory (Action: "DependencyAdded")
```

#### Reorder Tasks (Drag & Drop)
```csharp
// PUT /api/projects/{projectId}/tasks/reorder
✅ Validate all taskIds belong to project
✅ Update OrderIndex for each task
✅ Use transaction for batch update
✅ Log ActivityLog (Action: "TasksReordered")
```

---

### 💬 5. TASK COMMENTS

#### Add Comment
```csharp
// POST /api/tasks/{taskId}/comments
✅ Verify user can view task (team member)
✅ Create TaskComment with Content, UserId, TaskId
✅ Parse @mentions and create notifications
✅ Create TaskHistory (Action: "CommentAdded")
✅ Notify task participants (assignee, reporter)
```

#### Edit Comment
```csharp
// PUT /api/comments/{commentId}
✅ Verify ownership (UserId = currentUser.Id)
✅ Update Content, UpdatedAt
✅ Set IsEdited = true
✅ Create TaskHistory (Action: "CommentEdited")
```

#### Delete Comment
```csharp
// DELETE /api/comments/{commentId}
✅ Verify ownership or team owner
✅ Delete TaskComment (cascade handles history)
✅ Create TaskHistory (Action: "CommentDeleted")
```

---

### 🏷️ 6. TAGS & CATEGORIES

#### Create Tag
```csharp
// POST /api/teams/{teamId}/tags
✅ Verify caller is team member
✅ Validate name is unique per team (unique constraint)
✅ Create Tag with Color (default or provided)
✅ Log ActivityLog (Action: "TagCreated")
```

#### Create Project Category
```csharp
// POST /api/teams/{teamId}/categories
✅ Verify caller is TeamLead or Owner
✅ Validate name is unique per team
✅ Create ProjectCategory with Color
✅ Log ActivityLog (Action: "CategoryCreated")
```

#### Delete Tag
```csharp
// DELETE /api/tags/{tagId}
⚠️ Delete behavior is Restrict on TaskTag/ProjectTag
✅ First remove all TaskTag and ProjectTag entries
✅ Then delete Tag
✅ Log ActivityLog (Action: "TagDeleted")
```

---

### ⏱️ 7. TIME TRACKING

#### Log Time Entry
```csharp
// POST /api/tasks/{taskId}/time
✅ Verify user is team member
✅ Create TimeEntry with Hours, Description, EntryDate
✅ Recalculate task.ActualHours (SUM of all TimeEntries)
✅ Update task.UpdatedAt
✅ Create TaskHistory (Action: "TimeLogged", NewValue: hours)
```

#### Get Task Time Report
```csharp
// GET /api/tasks/{taskId}/time-report
✅ Query TimeEntries with UserId, Hours, EntryDate
✅ Calculate total hours per user
✅ Compare EstimatedHours vs ActualHours
✅ Return variance percentage
```

---

### 📝 8. AUDIT & ACTIVITY

#### Log Activity
```csharp
// Called internally on sensitive actions
✅ Create ActivityLog with UserId, Action, EntityType, EntityId
✅ Capture IpAddress from HttpContext
✅ Capture UserAgent from headers
✅ Store Details as JSON (old/new values)
```

#### View Activity Log (Admin)
```csharp
// GET /api/admin/activity-logs
✅ Filter by EntityType, EntityId, UserId, DateRange
✅ Include User navigation (FirstName, LastName)
✅ Paginate results (indexed on CreatedAt)
```

---

### 🔔 9. NOTIFICATIONS

#### Create Notification
```csharp
// POST /api/notifications (internal)
✅ Check user's NotificationSettings preferences
✅ Create Notification if InAppNotifications = true
✅ Send email if EmailNotifications = true
✅ Send push if PushNotifications = true
```

#### Mark as Read
```csharp
// PUT /api/notifications/{notificationId}/read
✅ Verify ownership (UserId = currentUser.Id)
✅ Set IsRead = true
```

#### Get Unread Count
```csharp
// GET /api/notifications/unread-count
✅ Query Notifications where UserId = currentUser.Id AND IsRead = false
✅ Return count
```

---

## Common Query Patterns

### Get User's Teams
```csharp
var teams = await _context.TeamUserRoles
    .Where(tur => tur.UserId == userId)
    .Include(tur => tur.Team)
    .Select(tur => tur.Team)
    .ToListAsync();
```

### Get Team Projects with Task Count
```csharp
var projects = await _context.Projects
    .Where(p => p.TeamId == teamId && !p.IsArchived)
    .Select(p => new {
        p.ProjectId,
        p.Name,
        p.Status,
        TaskCount = p.Tasks.Count(),
        CompletedTaskCount = p.Tasks.Count(t => t.Status == ProjectTaskStatus.Done)
    })
    .ToListAsync();
```

### Get Task with Full Details
```csharp
var task = await _context.Tasks
    .Include(t => t.AssignedUser)
    .Include(t => t.Reporter)
    .Include(t => t.Comments).ThenInclude(c => c.User)
    .Include(t => t.History).ThenInclude(h => h.User)
    .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
    .Include(t => t.Dependencies).ThenInclude(d => d.DependsOnTask)
    .Include(t => t.ParentTask)
    .Include(t => t.Subtasks)
    .FirstOrDefaultAsync(t => t.TaskId == taskId);
```

### Get User's Assigned Tasks Across All Teams
```csharp
var tasks = await _context.Tasks
    .Where(t => t.AssignedUserId == userId && t.Status != ProjectTaskStatus.Done)
    .Include(t => t.Project).ThenInclude(p => p.Team)
    .OrderBy(t => t.DueDate)
    .ToListAsync();
```

### Get Task History Timeline
```csharp
var history = await _context.TaskHistories
    .Where(h => h.TaskId == taskId)
    .Include(h => h.User)
    .OrderByDescending(h => h.CreatedAt)
    .ToListAsync();
```

### Search Tasks by Tag
```csharp
var tasks = await _context.TaskTags
    .Where(tt => tt.TagId == tagId)
    .Include(tt => tt.Task)
        .ThenInclude(t => t.AssignedUser)
    .Select(tt => tt.Task)
    .ToListAsync();
```

---

## Business Rules & Constraints

### Team Rules
- ❌ Cannot delete team with active projects
- ❌ Must have at least one TeamOwner
- ❌ Only TeamOwner/Lead can invite users
- ✅ Archive cascades to projects (optional)

### Project Rules
- ❌ Cannot change TeamId after creation
- ✅ Category must belong to same team
- ✅ Status: Planning → Active → Completed/OnHold/Cancelled
- ❌ Cannot delete project with tasks (cascade or move tasks)

### Task Rules
- ❌ Cannot complete task if blocked (IsBlocked = true)
- ❌ Cannot complete task if dependencies not done
- ✅ Subtasks inherit ProjectId from parent
- ✅ OrderIndex maintains drag-drop sequence
- ✅ Status transitions: Todo → InProgress → Review → Done

### Invitation Rules
- ✅ Token expires after 7 days (configurable)
- ❌ One pending invitation per email per team (unique constraint)
- ❌ Cannot invite existing members
- ✅ Expired invitations auto-marked as Expired (background job)

### Tag/Category Rules
- ✅ Names unique per team (case-insensitive recommended)
- ❌ Cannot delete tag/category with associations (Restrict)
- ✅ Colors stored as hex codes (#RRGGBB)

---

## Performance Optimization Tips

### Indexes Already in Place
✅ `Tasks`: Composite on (ProjectId, Status) - filtering tasks by project/status  
✅ `Tasks`: (AssignedUserId) - finding user's tasks  
✅ `Projects`: Composite on (TeamId, Status) - team project lists  
✅ `TaskHistory/TaskComment`: (CreatedAt) - timeline queries  
✅ `TeamInvitation`: (Token), (Email), (Status) - invitation lookups  
✅ `TeamUserRole`: Composite PK (UserId, TeamId) - membership checks  

### Query Optimization
```csharp
// ✅ GOOD: Select only needed columns
.Select(t => new TaskListDto { 
    TaskId = t.TaskId, 
    Title = t.Title, 
    Status = t.Status 
})

// ❌ BAD: Loading entire entity graph
.Include(t => t.Project).ThenInclude(p => p.Team).ThenInclude(...)

// ✅ GOOD: Paginated queries
.Skip((page - 1) * pageSize).Take(pageSize)

// ✅ GOOD: AsNoTracking for read-only queries
.AsNoTracking().Where(...)
```

### Caching Strategies
- Cache user team memberships (TTL: 5 minutes)
- Cache team tags/categories (invalidate on create/update/delete)
- Cache project list per team (invalidate on status change)
- Cache notification count (real-time via SignalR preferred)

### Background Jobs
- Expire old invitations (daily job)
- Archive completed projects (weekly job)
- Calculate project metrics (hourly job)
- Clean up soft-deleted users (monthly job)

---

## Delete Behavior Summary

| Relationship | Delete Behavior | Notes |
|-------------|----------------|-------|
| Team → Projects | Cascade | Deleting team deletes all projects |
| Project → Tasks | Cascade | Deleting project deletes all tasks |
| Task → Comments/History | Cascade | Deleting task deletes audit trail |
| Task → TimeEntries | Cascade | Deleting task deletes time logs |
| Tag → TaskTag/ProjectTag | **Restrict** | Must unlink before deleting tag |
| User → AssignedTasks | **Restrict** | Must reassign before deleting user |
| User → Teams Created | **Restrict** | Must transfer ownership first |
| Team → TeamCategories | Cascade | Deleting team deletes categories |
| ProjectCategory → Projects | **Restrict** | Must uncategorize before deleting |

⚠️ **Important**: Most User relationships use `Restrict` to prevent data loss. Implement "transfer ownership" or "reassign" flows before user deletion.

---

## API Endpoint Checklist

### Teams
- [ ] `POST /api/teams` - Create team
- [ ] `GET /api/teams` - List user's teams
- [ ] `GET /api/teams/{id}` - Get team details
- [ ] `PUT /api/teams/{id}` - Update team
- [ ] `DELETE /api/teams/{id}` - Delete team
- [ ] `PUT /api/teams/{id}/archive` - Archive team
- [ ] `GET /api/teams/{id}/members` - List members
- [ ] `POST /api/teams/{id}/invitations` - Invite user
- [ ] `DELETE /api/teams/{id}/members/{userId}` - Remove member

### Projects
- [ ] `POST /api/teams/{teamId}/projects` - Create project
- [ ] `GET /api/teams/{teamId}/projects` - List projects
- [ ] `GET /api/projects/{id}` - Get project details
- [ ] `PUT /api/projects/{id}` - Update project
- [ ] `PUT /api/projects/{id}/status` - Update status
- [ ] `DELETE /api/projects/{id}` - Delete project
- [ ] `POST /api/projects/{id}/tags` - Add tag
- [ ] `DELETE /api/projects/{id}/tags/{tagId}` - Remove tag

### Tasks
- [ ] `POST /api/projects/{projectId}/tasks` - Create task
- [ ] `GET /api/projects/{projectId}/tasks` - List tasks
- [ ] `GET /api/tasks/{id}` - Get task details
- [ ] `PUT /api/tasks/{id}` - Update task
- [ ] `PUT /api/tasks/{id}/status` - Update status
- [ ] `PUT /api/tasks/{id}/assign` - Assign user
- [ ] `DELETE /api/tasks/{id}` - Delete task
- [ ] `POST /api/tasks/{id}/comments` - Add comment
- [ ] `GET /api/tasks/{id}/history` - Get history
- [ ] `POST /api/tasks/{id}/time` - Log time
- [ ] `POST /api/tasks/{id}/dependencies` - Add dependency
- [ ] `GET /api/tasks/{id}/subtasks` - List subtasks

### Tags & Categories
- [ ] `POST /api/teams/{teamId}/tags` - Create tag
- [ ] `GET /api/teams/{teamId}/tags` - List tags
- [ ] `PUT /api/tags/{id}` - Update tag
- [ ] `DELETE /api/tags/{id}` - Delete tag
- [ ] `POST /api/teams/{teamId}/categories` - Create category
- [ ] `GET /api/teams/{teamId}/categories` - List categories
- [ ] `DELETE /api/categories/{id}` - Delete category

### Invitations
- [ ] `POST /api/invitations/accept` - Accept invitation
- [ ] `POST /api/invitations/{id}/cancel` - Cancel invitation
- [ ] `GET /api/invitations/pending` - List user's pending invitations

### Notifications
- [ ] `GET /api/notifications` - List notifications
- [ ] `GET /api/notifications/unread-count` - Get unread count
- [ ] `PUT /api/notifications/{id}/read` - Mark as read
- [ ] `PUT /api/notifications/read-all` - Mark all as read

### User Settings
- [ ] `GET /api/users/settings/notifications` - Get settings
- [ ] `PUT /api/users/settings/notifications` - Update settings

---

## Security Checklist

### Authorization Rules
- [ ] Only team members can view team data
- [ ] Only TeamLead/Owner can invite users
- [ ] Only TeamOwner can delete team
- [ ] Only task assignee/reporter/admin can edit task
- [ ] Only comment author can edit/delete comment
- [ ] API keys are hashed/encrypted
- [ ] Activity logs capture IP/UserAgent for audit

### Data Validation
- [ ] Validate email format in invitations
- [ ] Validate enum values (Status, Priority, Role)
- [ ] Sanitize HTML/scripts in comments/descriptions
- [ ] Validate date ranges (StartDate < EndDate)
- [ ] Validate time entries (Hours > 0)

---

## Testing Scenarios

### Unit Tests
- [ ] Task status transitions
- [ ] Circular dependency detection
- [ ] Invitation expiry logic
- [ ] Time calculation (EstimatedHours vs ActualHours)
- [ ] OrderIndex calculation

### Integration Tests
- [ ] Create team → invite user → accept → create project → create task flow
- [ ] Task assignment → comment → status change → completion flow
- [ ] Tag/category CRUD with cascade/restrict behavior
- [ ] Soft delete user and verify orphaned records handling

### Performance Tests
- [ ] Load 1000+ tasks per project
- [ ] Query user's tasks across 100+ teams
- [ ] Task history with 1000+ entries
- [ ] Concurrent task status updates

---

## Migration & Deployment Notes

✅ Migration already applied: `20260217065028_AddTeamForgeSchemaEnhancements`

### Post-Migration Tasks
- [ ] Seed default roles (SystemAdmin, SystemUser)
- [ ] Create sample team/project data for testing
- [ ] Set up background jobs for invitation expiry
- [ ] Configure notification email templates
- [ ] Set up monitoring for ActivityLogs

### Breaking Changes from Initial Schema
- `TeamUserRole.Role` now stored as **int** (was string)
- Added nullable FKs: `CreatedById`, `CategoryId`, `ReporterId`, `ParentTaskId`
- New indexes may require maintenance during deployment

---

## Summary

Your TeamsForge schema now supports:
✅ Multi-tenant team collaboration  
✅ Project/task management with subtasks  
✅ Complete audit trail (TaskHistory, ActivityLog)  
✅ Flexible tagging and categorization  
✅ Time tracking with estimates vs actuals  
✅ Invitation-based team onboarding  
✅ Notification preferences per user  
✅ API key management for integrations  
✅ Task dependencies and blocking  
✅ Soft delete for users  

**Next Steps**: Implement service layer, controllers, and DTOs following this guide. Use the query patterns provided for efficient data access.
