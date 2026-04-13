<template>
  <span
    class="material-symbols-outlined icon-ms"
    :class="className"
    :style="sizeStyle"
  >{{ materialIcon }}</span>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  icon: string
  size?: number | string
  strokeWidth?: number
  class?: string | string[] | Record<string, boolean>
}

const props = withDefaults(defineProps<Props>(), {
  size: 24,
  strokeWidth: 2,
  class: ''
})

const className = computed(() => {
  const cls = props.class
  if (Array.isArray(cls)) {
    return cls.join(' ')
  }
  if (typeof cls === 'object' && cls !== null) {
    return Object.entries(cls)
      .filter(([, value]) => value)
      .map(([key]) => key)
      .join(' ')
  }
  return cls
})

// Extract pixel size from class string for font-size
const sizeStyle = computed(() => {
  const cls = className.value

  // Check for arbitrary Tailwind w-[Xpx] pattern
  const arbitraryMatch = cls.match(/w-\[(\d+)px\]/)
  if (arbitraryMatch) {
    return { fontSize: arbitraryMatch[1] + 'px' }
  }

  // Map standard Tailwind w-* classes to pixel sizes
  // Order from most specific to least specific to avoid partial matches
  const sizeMap: [string, number][] = [
    ['w-16', 64], ['w-12', 48], ['w-10', 40], ['w-9', 36],
    ['w-8', 32], ['w-7', 28], ['w-6', 24], ['w-5', 20],
    ['w-4.5', 18], ['w-4', 16], ['w-3.5', 14], ['w-3', 12]
  ]

  for (const [twClass, px] of sizeMap) {
    // Use word boundary check to avoid w-3 matching w-3.5
    const regex = new RegExp(`\\b${twClass.replace('.', '\\.')}\\b`)
    if (regex.test(cls)) {
      return { fontSize: px + 'px' }
    }
  }

  // Default
  return { fontSize: '24px' }
})

// Map MDI icon names to Material Symbols names
const iconMap: Record<string, string> = {
  // Navigation & Actions
  'mdi:menu': 'menu',
  'mdi:menu-open': 'menu_open',
  'mdi:close': 'close',
  'mdi:check': 'check',
  'mdi:check-all': 'done_all',
  'mdi:check-circle': 'check_circle',
  'mdi:check-circle-outline': 'check_circle',
  'mdi:checkbox-marked-circle-outline': 'check_circle',
  'mdi:plus': 'add',
  'mdi:minus': 'remove',
  'mdi:delete': 'delete',
  'mdi:delete-outline': 'delete',
  'mdi:pencil': 'edit',
  'mdi:pencil-outline': 'edit',
  'mdi:edit': 'edit',
  'mdi:eye': 'visibility',
  'mdi:eye-off': 'visibility_off',
  'mdi:eye-outline': 'visibility',
  'mdi:eye-off-outline': 'visibility_off',
  'mdi:refresh': 'refresh',
  'mdi:reload': 'refresh',
  'mdi:sync': 'sync',
  'mdi:download': 'download',
  'mdi:download-outline': 'download',
  'mdi:upload': 'upload',
  'mdi:cloud-upload': 'cloud_upload',
  'mdi:cloud-upload-outline': 'cloud_upload',
  'mdi:share': 'share',
  'mdi:share-variant': 'share',
  'mdi:copy': 'content_copy',
  'mdi:content-copy': 'content_copy',
  'mdi:content-save': 'save',
  'mdi:content-save-outline': 'save',
  'mdi:magnify': 'search',
  'mdi:search': 'search',
  'mdi:text-search': 'manage_search',
  'mdi:book-search': 'manage_search',
  'mdi:filter': 'filter_list',
  'mdi:filter-outline': 'filter_list',
  'mdi:filter-variant': 'filter_list',
  'mdi:sort': 'sort',
  'mdi:sort-numeric-ascending': 'sort',
  'mdi:alphabetical': 'sort_by_alpha',
  'mdi:dots-vertical': 'more_vert',
  'mdi:dots-horizontal': 'more_horiz',
  'mdi:export': 'download',
  'mdi:import': 'upload',
  'mdi:print': 'print',
  'mdi:send': 'send',
  'mdi:send-check': 'send',

  // Arrows & Chevrons
  'mdi:arrow-left': 'arrow_back',
  'mdi:arrow-right': 'arrow_forward',
  'mdi:arrow-up': 'arrow_upward',
  'mdi:arrow-down': 'arrow_downward',
  'mdi:arrow-expand-all': 'open_in_full',
  'mdi:chevron-left': 'chevron_left',
  'mdi:chevron-right': 'chevron_right',
  'mdi:chevron-up': 'expand_less',
  'mdi:chevron-down': 'expand_more',
  'mdi:chevron-double-left': 'keyboard_double_arrow_left',
  'mdi:chevron-double-right': 'keyboard_double_arrow_right',
  'mdi:menu-left': 'chevron_left',
  'mdi:menu-right': 'chevron_right',
  'mdi:reply': 'reply',
  'mdi:transfer-up': 'move_up',

  // User & Account
  'mdi:account': 'person',
  'mdi:account-outline': 'person',
  'mdi:account-circle': 'account_circle',
  'mdi:account-circle-outline': 'account_circle',
  'mdi:account-group': 'group',
  'mdi:account-group-outline': 'group',
  'mdi:account-plus': 'person_add',
  'mdi:account-minus': 'person_remove',
  'mdi:account-edit': 'manage_accounts',
  'mdi:account-cog': 'manage_accounts',
  'mdi:account-check': 'how_to_reg',
  'mdi:account-check-outline': 'how_to_reg',
  'mdi:account-star': 'star',
  'mdi:account-star-outline': 'star',
  'mdi:account-search': 'person_search',
  'mdi:account-search-outline': 'person_search',
  'mdi:account-remove': 'person_off',
  'mdi:account-remove-outline': 'person_off',
  'mdi:account-cancel': 'person_off',
  'mdi:account-off': 'person_off',
  'mdi:account-off-outline': 'person_off',
  'mdi:account-key': 'key',
  'mdi:shield-account': 'admin_panel_settings',
  'mdi:badge-account': 'badge',
  'mdi:account-multiple': 'group',
  'mdi:account-multiple-outline': 'group',
  'mdi:card-account-details': 'badge',
  'mdi:card-account-details-outline': 'badge',

  // Calendar & Time
  'mdi:calendar': 'calendar_today',
  'mdi:calendar-outline': 'calendar_today',
  'mdi:calendar-blank': 'calendar_today',
  'mdi:calendar-blank-outline': 'calendar_today',
  'mdi:calendar-today': 'calendar_today',
  'mdi:calendar-month': 'calendar_month',
  'mdi:calendar-multiple': 'date_range',
  'mdi:calendar-plus': 'event',
  'mdi:calendar-edit': 'edit_calendar',
  'mdi:calendar-check': 'event_available',
  'mdi:calendar-check-outline': 'event_available',
  'mdi:calendar-clock': 'schedule',
  'mdi:calendar-remove': 'event_busy',
  'mdi:calendar-remove-outline': 'event_busy',
  'mdi:calendar-off': 'event_busy',
  'mdi:calendar-question': 'event_note',
  'mdi:clock': 'schedule',
  'mdi:clock-outline': 'schedule',
  'mdi:clock-start': 'schedule',
  'mdi:clock-end': 'schedule',
  'mdi:progress-clock': 'pending',
  'mdi:timer': 'timer',
  'mdi:timer-play': 'play_arrow',
  'mdi:history': 'history',

  // Files & Documents
  'mdi:file': 'description',
  'mdi:file-outline': 'description',
  'mdi:file-edit': 'edit_note',
  'mdi:file-edit-outline': 'edit_note',
  'mdi:file-document': 'description',
  'mdi:file-document-outline': 'description',
  'mdi:file-document-edit': 'edit_note',
  'mdi:file-document-edit-outline': 'edit_note',
  'mdi:file-document-multiple': 'file_copy',
  'mdi:file-document-multiple-outline': 'file_copy',
  'mdi:file-document-plus': 'note_add',
  'mdi:file-document-plus-outline': 'note_add',
  'mdi:file-document-remove': 'delete',
  'mdi:file-alert': 'file_present',
  'mdi:file-check': 'task',
  'mdi:file-check-outline': 'task',
  'mdi:file-question': 'help',
  'mdi:file-question-outline': 'help',
  'mdi:file-remove': 'delete',
  'mdi:page-layout-header': 'view_headline',
  'mdi:page-layout-body': 'view_module',
  'mdi:tag-text': 'label',
  'mdi:format-textdirection-r-to-l': 'format_textdirection_r_to_l',
  'mdi:file-pdf-box': 'picture_as_pdf',
  'mdi:file-excel': 'table_chart',
  'mdi:file-word': 'description',
  'mdi:file-image': 'image',
  'mdi:file-multiple': 'file_copy',
  'mdi:file-plus': 'note_add',
  'mdi:file-download': 'file_download',
  'mdi:file-upload': 'file_upload',
  'mdi:file-chart': 'insert_chart',
  'mdi:file-chart-outline': 'insert_chart',
  'mdi:file-eye': 'preview',
  'mdi:file-eye-outline': 'preview',
  'mdi:folder': 'folder',
  'mdi:folder-outline': 'folder',
  'mdi:folder-open': 'folder_open',
  'mdi:folder-open-outline': 'folder_open',
  'mdi:folder-off-outline': 'folder_off',
  'mdi:folder-plus': 'create_new_folder',
  'mdi:folder-search': 'folder',
  'mdi:folder-search-outline': 'folder',
  'mdi:folder-account': 'folder_shared',
  'mdi:folder-account-outline': 'folder_shared',
  'mdi:folder-key': 'folder_shared',
  'mdi:folder-key-outline': 'folder_shared',
  'mdi:folder-multiple': 'folder_copy',
  'mdi:folder-multiple-outline': 'folder_copy',
  'mdi:folder-network': 'folder_shared',
  'mdi:file-tree': 'account_tree',
  'mdi:file-tree-outline': 'account_tree',
  'mdi:attachment': 'attach_file',
  'mdi:paperclip': 'attach_file',
  'mdi:link': 'link',
  'mdi:link-variant': 'link',
  'mdi:link-variant-off': 'link_off',
  'mdi:link-variant-remove': 'link_off',

  // Communication
  'mdi:email': 'mail',
  'mdi:email-outline': 'mail',
  'mdi:email-edit': 'edit_note',
  'mdi:email-edit-outline': 'edit_note',
  'mdi:email-off-outline': 'unsubscribe',
  'mdi:email-fast': 'forward_to_inbox',
  'mdi:phone': 'phone',
  'mdi:phone-outline': 'phone',
  'mdi:message': 'chat',
  'mdi:message-outline': 'chat',
  'mdi:message-text': 'chat',
  'mdi:chat': 'chat',
  'mdi:chat-outline': 'chat',
  'mdi:comment': 'comment',
  'mdi:comment-outline': 'comment',
  'mdi:comment-text': 'comment',
  'mdi:bell': 'notifications',
  'mdi:bell-outline': 'notifications',
  'mdi:bell-ring': 'notifications_active',
  'mdi:bell-off': 'notifications_off',

  // Settings & Tools
  'mdi:cog': 'settings',
  'mdi:cog-outline': 'settings',
  'mdi:cog-off': 'settings',
  'mdi:settings': 'settings',
  'mdi:wrench': 'build',
  'mdi:tools': 'build',
  'mdi:tune': 'tune',
  'mdi:tune-vertical': 'tune',
  'mdi:key': 'key',
  'mdi:key-variant': 'key',
  'mdi:lock': 'lock',
  'mdi:lock-outline': 'lock',
  'mdi:lock-open': 'lock_open',
  'mdi:lock-open-outline': 'lock_open',
  'mdi:lock-reset': 'lock_reset',
  'mdi:shield': 'shield',
  'mdi:shield-outline': 'shield',
  'mdi:shield-check': 'verified_user',
  'mdi:shield-check-outline': 'verified_user',
  'mdi:shield-edit': 'admin_panel_settings',
  'mdi:shield-off': 'remove_moderator',
  'mdi:shield-off-outline': 'remove_moderator',
  'mdi:shield-remove': 'remove_moderator',
  'mdi:shield-remove-outline': 'remove_moderator',
  'mdi:shield-crown': 'verified_user',
  'mdi:shield-crown-outline': 'verified_user',
  'mdi:shield-key': 'admin_panel_settings',
  'mdi:shield-key-outline': 'admin_panel_settings',
  'mdi:shield-lock': 'admin_panel_settings',

  // Status & Alerts
  'mdi:information': 'info',
  'mdi:information-outline': 'info',
  'mdi:information-variant': 'info',
  'mdi:alert': 'warning',
  'mdi:alert-outline': 'warning',
  'mdi:alert-circle': 'error',
  'mdi:alert-circle-outline': 'error',
  'mdi:help-circle': 'help',
  'mdi:help-circle-outline': 'help',
  'mdi:close-circle': 'cancel',
  'mdi:close-circle-outline': 'cancel',
  'mdi:check-outline': 'check',
  'mdi:circle': 'circle',
  'mdi:circle-outline': 'circle',
  'mdi:record-circle': 'radio_button_checked',
  'mdi:checkbox-marked-circle': 'check_circle',
  'mdi:checkbox-blank-circle-outline': 'radio_button_unchecked',

  // Badges & Verification
  'mdi:check-decagram': 'verified',
  'mdi:check-decagram-outline': 'verified',
  'mdi:decagram': 'verified',
  'mdi:decagram-outline': 'verified',

  // Business & Organization
  'mdi:domain': 'domain',
  'mdi:office-building': 'business',
  'mdi:sitemap': 'account_tree',
  'mdi:sitemap-outline': 'account_tree',
  'mdi:organization': 'business',
  'mdi:briefcase': 'work',
  'mdi:briefcase-outline': 'work',
  'mdi:badge': 'badge',
  'mdi:certificate': 'workspace_premium',

  // Data & Charts
  'mdi:chart-bar': 'bar_chart',
  'mdi:chart-bar-stacked': 'bar_chart',
  'mdi:chart-box': 'bar_chart',
  'mdi:chart-line': 'show_chart',
  'mdi:chart-areaspline': 'area_chart',
  'mdi:chart-pie': 'pie_chart',
  'mdi:chart-donut': 'donut_large',
  'mdi:chart-arc': 'pie_chart',
  'mdi:chart-timeline-variant': 'timeline',
  'mdi:chart-timeline': 'timeline',
  'mdi:timeline-clock': 'timeline',
  'mdi:timeline-clock-outline': 'timeline',
  'mdi:chart-gantt': 'view_timeline',
  'mdi:trending-up': 'trending_up',
  'mdi:trending-down': 'trending_down',
  'mdi:percent': 'percent',
  'mdi:poll': 'poll',
  'mdi:finance': 'payments',
  'mdi:cash': 'payments',

  // Lists & Tables
  'mdi:format-list-bulleted': 'format_list_bulleted',
  'mdi:format-list-numbered': 'format_list_numbered',
  'mdi:format-list-checks': 'checklist',
  'mdi:format-list-bulleted-type': 'list',
  'mdi:view-list': 'view_list',
  'mdi:view-grid': 'grid_view',
  'mdi:view-grid-outline': 'grid_view',
  'mdi:view-dashboard': 'dashboard',
  'mdi:view-dashboard-outline': 'dashboard',
  'mdi:table': 'table_chart',
  'mdi:table-large': 'table_chart',
  'mdi:reorder-horizontal': 'drag_indicator',
  'mdi:drag': 'drag_indicator',
  'mdi:drag-horizontal': 'drag_indicator',
  'mdi:drag-vertical': 'drag_indicator',

  // Navigation & Location
  'mdi:home': 'home',
  'mdi:home-outline': 'home',
  'mdi:map-marker': 'location_on',
  'mdi:map-marker-outline': 'location_on',
  'mdi:map': 'map',
  'mdi:compass': 'explore',
  'mdi:navigation': 'navigation',
  'mdi:door': 'door_front',
  'mdi:door-open': 'door_front',
  'mdi:door-closed': 'door_front',

  // Media
  'mdi:image': 'image',
  'mdi:image-outline': 'image',
  'mdi:camera': 'photo_camera',
  'mdi:video': 'videocam',
  'mdi:video-outline': 'videocam',
  'mdi:play': 'play_arrow',
  'mdi:play-circle': 'play_circle',
  'mdi:play-circle-outline': 'play_circle',
  'mdi:pause': 'pause',
  'mdi:pause-circle': 'pause_circle',
  'mdi:stop': 'stop',
  'mdi:stop-circle': 'stop_circle',
  'mdi:stop-circle-outline': 'stop_circle',
  'mdi:microphone': 'mic',
  'mdi:microphone-off': 'mic_off',

  // Text & Format
  'mdi:format-title': 'title',
  'mdi:format-quote-close': 'format_quote',
  'mdi:format-strikethrough': 'strikethrough_s',
  'mdi:format-bold': 'format_bold',
  'mdi:format-italic': 'format_italic',
  'mdi:format-underline': 'format_underlined',
  'mdi:format-align-left': 'format_align_left',
  'mdi:format-align-center': 'format_align_center',
  'mdi:format-align-right': 'format_align_right',
  'mdi:text': 'text_fields',
  'mdi:text-box': 'text_fields',
  'mdi:text-box-outline': 'text_fields',
  'mdi:note': 'note',
  'mdi:note-outline': 'note',
  'mdi:note-text': 'description',
  'mdi:note-text-outline': 'description',

  // Meeting specific
  'mdi:vote': 'how_to_vote',
  'mdi:vote-outline': 'how_to_vote',
  'mdi:gavel': 'gavel',
  'mdi:presentation': 'co_present',
  'mdi:stamp': 'approval',
  'mdi:hand-wave': 'waving_hand',
  'mdi:handshake': 'handshake',
  'mdi:handshake-outline': 'handshake',
  'mdi:arrow-up-bold-circle-outline': 'arrow_circle_up',
  'mdi:arrow-down-bold-circle-outline': 'arrow_circle_down',
  'mdi:arrow-up-circle': 'arrow_circle_up',
  'mdi:arrow-down-circle': 'arrow_circle_down',
  'mdi:comment-text-outline': 'comment',
  'mdi:clipboard-list-outline': 'assignment',
  'mdi:calendar-range': 'date_range',
  'mdi:calendar-range-outline': 'date_range',
  'mdi:account-multiple-outline': 'group',
  'mdi:format-list-bulleted-type': 'list_alt',
  'mdi:tag-multiple': 'sell',
  'mdi:tag-multiple-outline': 'sell',
  'mdi:tag-outline': 'sell',
  'mdi:tag-plus-outline': 'new_label',
  'mdi:account-group-outline': 'groups',
  'mdi:link-variant': 'link',
  'mdi:pound': 'tag',
  'mdi:lock-outline': 'lock',
  'mdi:signature': 'draw',
  'mdi:signature-freehand': 'draw',
  'mdi:draw': 'draw',

  // Misc
  'mdi:star': 'star',
  'mdi:star-outline': 'star',
  'mdi:heart': 'favorite',
  'mdi:heart-outline': 'favorite',
  'mdi:bookmark': 'bookmark',
  'mdi:bookmark-outline': 'bookmark',
  'mdi:tag': 'label',
  'mdi:tag-outline': 'label',
  'mdi:flag': 'flag',
  'mdi:flag-outline': 'flag',
  'mdi:pin': 'push_pin',
  'mdi:pin-outline': 'push_pin',
  'mdi:pound': 'tag',
  'mdi:identifier': 'tag',
  'mdi:at': 'alternate_email',
  'mdi:database': 'storage',
  'mdi:database-outline': 'storage',
  'mdi:database-off': 'storage',
  'mdi:database-off-outline': 'storage',
  'mdi:server': 'dns',
  'mdi:cloud': 'cloud',
  'mdi:cloud-outline': 'cloud',
  'mdi:power': 'power_settings_new',
  'mdi:logout': 'logout',
  'mdi:login': 'login',
  'mdi:exit-to-app': 'logout',
  'mdi:launch': 'open_in_new',
  'mdi:open-in-new': 'open_in_new',
  'mdi:loading': 'progress_activity',
  'mdi:cached': 'refresh',
  'mdi:fullscreen': 'fullscreen',
  'mdi:fullscreen-exit': 'fullscreen_exit',
  'mdi:expand': 'open_in_full',
  'mdi:collapse': 'close_fullscreen',
  'mdi:square-edit-outline': 'edit_square',
  'mdi:approve': 'thumb_up',
  'mdi:cancel': 'block',
  'mdi:checkbox-marked': 'check_box',
  'mdi:checkbox-marked-outline': 'check_box',
  'mdi:checkbox-blank': 'check_box_outline_blank',
  'mdi:checkbox-blank-outline': 'check_box_outline_blank',
  'mdi:radiobox-marked': 'radio_button_checked',
  'mdi:radiobox-blank': 'radio_button_unchecked',
  'mdi:toggle-switch': 'toggle_on',
  'mdi:toggle-switch-off': 'toggle_off',
  'mdi:web': 'language',
  'mdi:earth': 'language',
  'mdi:translate': 'translate',
  'mdi:abjad-arabic': 'translate',
  'mdi:clipboard': 'content_paste',
  'mdi:clipboard-outline': 'content_paste',
  'mdi:clipboard-check': 'assignment_turned_in',
  'mdi:clipboard-check-outline': 'assignment_turned_in',
  'mdi:clipboard-check-multiple': 'assignment',
  'mdi:clipboard-clock': 'assignment',
  'mdi:clipboard-text': 'assignment',
  'mdi:clipboard-text-outline': 'assignment',
  'mdi:clipboard-list': 'assignment',
  'mdi:clipboard-remove': 'assignment_late',
  'mdi:clipboard-remove-outline': 'assignment_late',
  'mdi:content-cut': 'content_cut',
  'mdi:content-paste': 'content_paste',
  'mdi:inbox': 'inbox',
  'mdi:archive': 'archive',
  'mdi:trash-can': 'delete',
  'mdi:trash-can-outline': 'delete',
  'mdi:restore': 'restore',
  'mdi:undo': 'undo',
  'mdi:redo': 'redo',
  'mdi:zoom-in': 'zoom_in',
  'mdi:zoom-out': 'zoom_out',
  'mdi:thumbs-up': 'thumb_up',
  'mdi:thumbs-down': 'thumb_down',
  'mdi:thumb-up': 'thumb_up',
  'mdi:thumb-down': 'thumb_down',
  'mdi:lightbulb': 'lightbulb',
  'mdi:lightbulb-outline': 'lightbulb',
  'mdi:lightbulb-on': 'lightbulb',
  'mdi:lightbulb-on-outline': 'lightbulb',
  'mdi:lightbulb-off': 'lightbulb',
  'mdi:lightbulb-off-outline': 'lightbulb',

  // Microsoft
  'mdi:microsoft': 'window',
  'mdi:microsoft-outlook': 'mail',
  'mdi:microsoft-azure': 'cloud',
  'mdi:microsoft-teams': 'groups',
  'mdi:connection': 'cable',
  'mdi:book-open-variant': 'menu_book',
  'mdi:book-open-page-variant': 'menu_book',
  'mdi:numeric': 'tag',
  'mdi:application': 'apps',
  'mdi:apps': 'apps',
  'mdi:code-tags': 'code',
  'mdi:form-select': 'list_alt',
  'mdi:lightning-bolt': 'bolt',
  'mdi:lightning-bolt-outline': 'bolt',
  'mdi:monitor-account': 'desktop_windows',
  'mdi:music-note': 'music_note',
  'mdi:printer': 'print',
  'mdi:publish': 'publish',
  'mdi:rocket-launch': 'rocket_launch',
  'mdi:send-circle': 'send',
  'mdi:skip-next-circle': 'skip_next',
  'mdi:source-branch': 'fork_right',
  'mdi:star-check': 'stars',
  'mdi:test-tube': 'science',
}

const materialIcon = computed(() => {
  const iconName = props.icon

  // Try direct map lookup
  const mapped = iconMap[iconName]
  if (mapped) return mapped

  // If starts with mdi:, try to convert to material symbols name
  if (iconName.startsWith('mdi:')) {
    const name = iconName.replace('mdi:', '')
    // Convert kebab-case to underscore_case for Material Symbols
    return name.replace(/-/g, '_')
  }

  // Already a material symbols name
  return iconName
})
</script>

<style scoped>
.icon-ms {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
}
</style>
