<template>
  <div class="text-editor">
    <label v-if="label" class="block text-sm font-medium text-zinc-700 mb-2">
      {{ label }}
    </label>

    <!-- Toolbar -->
    <div
      v-if="!readonly && editor"
      class="border border-gray-300 border-b-0 rounded-t-lg bg-zinc-50 px-2 py-1 flex flex-wrap items-center gap-1"
    >
      <!-- Text Formatting -->
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive('bold') }]"
        @click="editor?.chain().focus().toggleBold().run()"
      >
        <Icon icon="mdi:format-bold" class="w-4 h-4" />
      </button>
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive('italic') }]"
        @click="editor?.chain().focus().toggleItalic().run()"
      >
        <Icon icon="mdi:format-italic" class="w-4 h-4" />
      </button>
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive('underline') }]"
        @click="editor?.chain().focus().toggleUnderline().run()"
      >
        <Icon icon="mdi:format-underline" class="w-4 h-4" />
      </button>
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive('strike') }]"
        @click="editor?.chain().focus().toggleStrike().run()"
      >
        <Icon icon="mdi:format-strikethrough" class="w-4 h-4" />
      </button>

      <div class="w-px h-6 bg-zinc-300 mx-1" />

      <!-- Headings -->
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive('heading', { level: 1 }) }]"
        @click="editor?.chain().focus().toggleHeading({ level: 1 }).run()"
      >
        <span class="text-xs font-bold">H1</span>
      </button>
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive('heading', { level: 2 }) }]"
        @click="editor?.chain().focus().toggleHeading({ level: 2 }).run()"
      >
        <span class="text-xs font-bold">H2</span>
      </button>
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive('heading', { level: 3 }) }]"
        @click="editor?.chain().focus().toggleHeading({ level: 3 }).run()"
      >
        <span class="text-xs font-bold">H3</span>
      </button>

      <div class="w-px h-6 bg-zinc-300 mx-1" />

      <!-- Lists -->
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive('bulletList') }]"
        @click="editor?.chain().focus().toggleBulletList().run()"
      >
        <Icon icon="mdi:format-list-bulleted" class="w-4 h-4" />
      </button>
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive('orderedList') }]"
        @click="editor?.chain().focus().toggleOrderedList().run()"
      >
        <Icon icon="mdi:format-list-numbered" class="w-4 h-4" />
      </button>

      <div class="w-px h-6 bg-zinc-300 mx-1" />

      <!-- Alignment -->
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive({ textAlign: 'right' }) }]"
        @click="editor?.chain().focus().setTextAlign('right').run()"
      >
        <Icon icon="mdi:format-align-right" class="w-4 h-4" />
      </button>
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive({ textAlign: 'center' }) }]"
        @click="editor?.chain().focus().setTextAlign('center').run()"
      >
        <Icon icon="mdi:format-align-center" class="w-4 h-4" />
      </button>
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive({ textAlign: 'left' }) }]"
        @click="editor?.chain().focus().setTextAlign('left').run()"
      >
        <Icon icon="mdi:format-align-left" class="w-4 h-4" />
      </button>

      <div class="w-px h-6 bg-zinc-300 mx-1" />

      <!-- Other -->
      <button
        type="button"
        :class="['p-1.5 rounded hover:bg-zinc-200', { 'bg-zinc-200': editor?.isActive('blockquote') }]"
        @click="editor?.chain().focus().toggleBlockquote().run()"
      >
        <Icon icon="mdi:format-quote-close" class="w-4 h-4" />
      </button>
      <button
        type="button"
        class="p-1.5 rounded hover:bg-zinc-200"
        @click="editor?.chain().focus().setHorizontalRule().run()"
      >
        <Icon icon="mdi:minus" class="w-4 h-4" />
      </button>

      <div class="w-px h-6 bg-zinc-300 mx-1" />

      <!-- Undo/Redo -->
      <button
        type="button"
        class="p-1.5 rounded hover:bg-zinc-200"
        :disabled="!editor?.can().undo()"
        @click="editor?.chain().focus().undo().run()"
      >
        <Icon icon="mdi:undo" class="w-4 h-4" />
      </button>
      <button
        type="button"
        class="p-1.5 rounded hover:bg-zinc-200"
        :disabled="!editor?.can().redo()"
        @click="editor?.chain().focus().redo().run()"
      >
        <Icon icon="mdi:redo" class="w-4 h-4" />
      </button>
    </div>

    <!-- Editor Content -->
    <EditorContent
      :editor="editor"
      :class="[
        'border border-gray-300 prose prose-sm max-w-none',
        readonly ? 'rounded-lg' : 'rounded-b-lg',
        'min-h-[200px] focus-within:border-primary'
      ]"
    />

    <!-- Error message -->
    <p v-if="error" class="mt-1 text-sm text-error">{{ error }}</p>
  </div>
</template>

<script setup lang="ts">
import { watch, onBeforeUnmount } from 'vue'
import { useEditor, EditorContent } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Underline from '@tiptap/extension-underline'
import TextAlign from '@tiptap/extension-text-align'
import Icon from '@/components/ui/Icon.vue'

const props = withDefaults(
  defineProps<{
    modelValue: string
    label?: string
    placeholder?: string
    readonly?: boolean
    error?: string
  }>(),
  {
    modelValue: '',
    readonly: false
  }
)

const emit = defineEmits(['update:modelValue'])

const editor = useEditor({
  content: props.modelValue,
  editable: !props.readonly,
  extensions: [
    StarterKit,
    Underline,
    TextAlign.configure({
      types: ['heading', 'paragraph']
    })
  ],
  editorProps: {
    attributes: {
      class: 'p-4 focus:outline-none min-h-[200px]'
    }
  },
  onUpdate: ({ editor }) => {
    emit('update:modelValue', editor.getHTML())
  }
})

// Watch for external changes
watch(
  () => props.modelValue,
  (value) => {
    if (editor.value && value !== editor.value.getHTML()) {
      editor.value.commands.setContent(value, false)
    }
  }
)

// Watch for readonly changes
watch(
  () => props.readonly,
  (readonly) => {
    editor.value?.setEditable(!readonly)
  }
)

onBeforeUnmount(() => {
  editor.value?.destroy()
})
</script>

<style>
.ProseMirror {
  min-height: 200px;
}

.ProseMirror p.is-editor-empty:first-child::before {
  color: #adb5bd;
  content: attr(data-placeholder);
  float: left;
  height: 0;
  pointer-events: none;
}
</style>
