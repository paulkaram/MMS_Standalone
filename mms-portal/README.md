# MMS Vue 3 Migration

Meeting Management System (نظام إدارة الاجتماعات) - Vue 3 + Tailwind CSS

## Overview

This is the Vue 3 migration of the MMS application, migrating from:
- **Vue 2.6.14** → **Vue 3.4+**
- **Vuetify 2.7.0** → **Tailwind CSS + Headless UI + PrimeVue**
- **Vuex 3.6.2** → **Pinia**
- **Vue Router 3.6.5** → **Vue Router 4**
- **vue-i18n 8.28.2** → **vue-i18n 9**

## Quick Start

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Type check
npm run type-check
```

## Project Structure

```
src/
├── assets/
│   └── css/
│       └── main.css         # Tailwind CSS + custom styles
├── components/
│   ├── app/                  # Application-specific components
│   │   └── dashboard/
│   ├── controls/             # Control components (GridWithServerPaging, etc.)
│   ├── layout/               # Layout components (Header, Sidebar)
│   └── ui/                   # Reusable UI components
│       ├── Button.vue
│       ├── Card.vue
│       ├── Combobox.vue
│       ├── DataTable.vue
│       ├── DatePicker.vue
│       ├── HijriDatePicker.vue
│       ├── Input.vue
│       ├── Modal.vue
│       └── Select.vue
├── composables/
│   ├── index.ts
│   └── useSignalR.ts         # SignalR real-time communication
├── pages/                    # Page components (routes)
│   ├── Admin/
│   ├── Dashboard/
│   ├── Meetings/
│   ├── Reports/
│   └── ...
├── plugins/
│   ├── axios/               # Axios HTTP client
│   └── i18n/                # Internationalization
├── router/
│   └── index.ts             # Vue Router configuration
├── services/                 # API services
├── stores/
│   ├── index.ts
│   ├── app.ts               # App state (theme, sidebar, etc.)
│   └── user.ts              # User authentication state
├── App.vue
└── main.ts
```

## Key Features

### RTL Support
Full RTL (Right-to-Left) support for Arabic language:
- Tailwind RTL plugin (`tailwindcss-rtl`)
- Direction-aware spacing utilities (ps, pe, ms, me)
- Automatic flipping of layouts

### Hijri Calendar
Custom Hijri date picker using `moment-hijri`:
- Toggle between Hijri and Gregorian calendars
- Proper Arabic month names
- Date conversion display

### State Management (Pinia)
```typescript
// Using the user store
import { useUserStore } from '@/stores/user'

const userStore = useUserStore()
const isAuthenticated = userStore.isAuthenticated
const user = userStore.loggedInUser
```

### SignalR Integration
```typescript
// Using SignalR composable
import { useSignalR, MeetingHubEvents } from '@/composables/useSignalR'

const { connect, on, invoke, isConnected } = useSignalR()

await connect()
on(MeetingHubEvents.MEETING_STARTED, (data) => {
  console.log('Meeting started:', data)
})
```

### UI Components

#### Button
```vue
<Button variant="primary" icon-left="mdi:plus" :loading="isLoading">
  Add Meeting
</Button>
```

#### Input
```vue
<Input
  v-model="email"
  type="email"
  label="Email"
  icon-left="mdi:email"
  :error="errors.email"
/>
```

#### Select
```vue
<Select
  v-model="selectedRole"
  :options="roles"
  item-text="name"
  item-value="id"
  label="Role"
  filterable
/>
```

#### HijriDatePicker
```vue
<HijriDatePicker
  v-model="meetingDate"
  label="Meeting Date"
  :default-to-hijri="true"
  show-toggle
/>
```

#### DataTable (PrimeVue)
```vue
<DataTable
  :data="meetings"
  :columns="columns"
  :loading="loading"
  :total-records="totalRecords"
  server-side
  @page="handlePage"
  @sort="handleSort"
>
  <template #body-status="{ data }">
    <Badge :variant="getStatusVariant(data.status)">
      {{ data.status }}
    </Badge>
  </template>
</DataTable>
```

#### GridWithServerPaging (Legacy API)
```vue
<GridWithServerPaging
  :items="items"
  :headers="headers"
  :total-items="totalItems"
  :loading="loading"
  searchable
  show-add-button
  @load="loadData"
  @add="handleAdd"
>
  <template #item.status="{ value }">
    <Badge>{{ value }}</Badge>
  </template>

  <template #actions="{ item }">
    <Button variant="ghost" @click="edit(item)">Edit</Button>
  </template>
</GridWithServerPaging>
```

## Environment Variables

Create `.env.local` for local overrides:

```env
VITE_MAIN_API=http://localhost:1010/api/
VITE_COOKIE_TIMEOUT=6h
VITE_DEFAULT_LANGUAGE=ar
VITE_IAM_AUTHORITY=http://localhost:7077/
VITE_IAM_CLIENT_ID=your-client-id
VITE_IAM_REDIRECT=http://localhost:8080/signIn-oidc
```

## Migration Notes

### Vuetify → Tailwind Component Mapping

| Vuetify | New Component | Notes |
|---------|---------------|-------|
| `v-btn` | `<Button>` | Uses Tailwind + Headless UI |
| `v-text-field` | `<Input>` | Built with Tailwind |
| `v-select` | `<Select>` | Uses Headless UI Listbox |
| `v-autocomplete` | `<Combobox>` | Uses Headless UI Combobox |
| `v-dialog` | `<Modal>` | Uses Headless UI Dialog |
| `v-data-table` | `<DataTable>` | Uses PrimeVue DataTable |
| `v-card` | `<Card>` | Built with Tailwind |
| `v-tabs` | Headless UI TabGroup | Direct usage |
| `v-menu` | Headless UI Menu | Direct usage |

### Vuex → Pinia Migration

```javascript
// Before (Vuex)
this.$store.dispatch('user/setUserSession', payload)
this.$store.getters['user/isAuthenticated']

// After (Pinia)
const userStore = useUserStore()
userStore.setUserSession(payload)
userStore.isAuthenticated
```

### Vue Router Migration

```javascript
// Before (Vue Router 3)
new Router({ mode: 'history', routes })

// After (Vue Router 4)
createRouter({
  history: createWebHistory(),
  routes
})
```

### Options API → Composition API

```vue
<!-- Before -->
<script>
export default {
  data() { return { count: 0 } },
  computed: { double() { return this.count * 2 } },
  methods: { increment() { this.count++ } }
}
</script>

<!-- After -->
<script setup>
import { ref, computed } from 'vue'
const count = ref(0)
const double = computed(() => count.value * 2)
function increment() { count.value++ }
</script>
```

## Development Guidelines

1. **Use TypeScript** for all new code
2. **Use Composition API** with `<script setup>`
3. **Follow Tailwind conventions** for styling
4. **Use Pinia stores** for global state
5. **Create composables** for reusable logic
6. **Keep components small** and focused

## Testing

```bash
# Run unit tests
npm run test:unit

# Run e2e tests
npm run test:e2e
```

## Building for Production

```bash
# Build
npm run build

# Preview production build
npm run preview
```

## License

Proprietary - All rights reserved
