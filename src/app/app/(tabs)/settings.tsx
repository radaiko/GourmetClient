import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useAuthStore } from '../../src-rn/store/authStore';
import { useVentopayAuthStore } from '../../src-rn/store/ventopayAuthStore';
import { isDesktop } from '../../src-rn/utils/platform';
import { checkForDesktopUpdates } from '../../src-rn/utils/desktopUpdater';
import { useTheme } from '../../src-rn/theme/useTheme';
import { useDesktopLayout } from '../../src-rn/hooks/useDesktopLayout';
import { useThemeStore, ThemePreference } from '../../src-rn/store/themeStore';
import { Colors } from '../../src-rn/theme/colors';
import {
  inputField,
  buttonPrimary,
  buttonSecondary,
  buttonDanger,
  tintedBanner,
  bannerSurface,
  cardSurface,
} from '../../src-rn/theme/platformStyles';

const THEME_OPTIONS: { value: ThemePreference; label: string }[] = [
  { value: 'system', label: 'System' },
  { value: 'light', label: 'Light' },
  { value: 'dark', label: 'Dark' },
];

export default function SettingsScreen() {
  const { colors } = useTheme();
  const insets = useSafeAreaInsets();
  const { isWideLayout } = useDesktopLayout();
  const styles = useMemo(() => createStyles(colors), [colors]);

  const themePreference = useThemeStore((s) => s.preference);
  const setThemePreference = useThemeStore((s) => s.setPreference);

  // Gourmet auth
  const {
    status: gourmetStatus,
    error: gourmetError,
    userInfo,
    login: gourmetLogin,
    logout: gourmetLogout,
    saveCredentials: gourmetSaveCredentials,
    getSavedCredentials: gourmetGetSavedCredentials,
  } = useAuthStore();

  // Ventopay auth
  const {
    status: ventopayStatus,
    error: ventopayError,
    login: ventopayLogin,
    logout: ventopayLogout,
    saveCredentials: ventopaySaveCredentials,
    getSavedCredentials: ventopayGetSavedCredentials,
  } = useVentopayAuthStore();

  // Gourmet form state
  const [gUsername, setGUsername] = useState('');
  const [gPassword, setGPassword] = useState('');
  const [gTestStatus, setGTestStatus] = useState<'idle' | 'testing' | 'success' | 'error'>('idle');
  const [gSaving, setGSaving] = useState(false);

  // Ventopay form state
  const [vUsername, setVUsername] = useState('');
  const [vPassword, setVPassword] = useState('');
  const [vTestStatus, setVTestStatus] = useState<'idle' | 'testing' | 'success' | 'error'>('idle');
  const [vSaving, setVSaving] = useState(false);

  // Desktop update state
  const [checkingUpdates, setCheckingUpdates] = useState(false);

  // Load saved credentials on mount
  useEffect(() => {
    (async () => {
      const gCreds = await gourmetGetSavedCredentials();
      if (gCreds) {
        setGUsername(gCreds.username);
        setGPassword(gCreds.password);
      }
      const vCreds = await ventopayGetSavedCredentials();
      if (vCreds) {
        setVUsername(vCreds.username);
        setVPassword(vCreds.password);
      }
    })();
  }, [gourmetGetSavedCredentials, ventopayGetSavedCredentials]);

  // Gourmet handlers
  const handleGourmetTest = async () => {
    if (!gUsername || !gPassword) {
      Alert.alert('Error', 'Please enter username and password');
      return;
    }
    setGTestStatus('testing');
    const success = await gourmetLogin(gUsername, gPassword);
    setGTestStatus(success ? 'success' : 'error');
  };

  const handleGourmetSave = async () => {
    if (!gUsername || !gPassword) {
      Alert.alert('Error', 'Please enter username and password');
      return;
    }
    setGSaving(true);
    await gourmetSaveCredentials(gUsername, gPassword);
    await gourmetLogin(gUsername, gPassword);
    setGSaving(false);
    Alert.alert('Saved', 'Gourmet credentials saved securely');
  };

  const handleGourmetLogout = async () => {
    await gourmetLogout();
    setGTestStatus('idle');
  };

  // Ventopay handlers
  const handleVentopayTest = async () => {
    if (!vUsername || !vPassword) {
      Alert.alert('Error', 'Please enter username and password');
      return;
    }
    setVTestStatus('testing');
    const success = await ventopayLogin(vUsername, vPassword);
    setVTestStatus(success ? 'success' : 'error');
  };

  const handleVentopaySave = async () => {
    if (!vUsername || !vPassword) {
      Alert.alert('Error', 'Please enter username and password');
      return;
    }
    setVSaving(true);
    await ventopaySaveCredentials(vUsername, vPassword);
    await ventopayLogin(vUsername, vPassword);
    setVSaving(false);
    Alert.alert('Saved', 'Ventopay credentials saved securely');
  };

  const handleVentopayLogout = async () => {
    await ventopayLogout();
    setVTestStatus('idle');
  };

  const handleCheckForUpdates = async () => {
    setCheckingUpdates(true);
    try {
      await checkForDesktopUpdates(true);
    } finally {
      setCheckingUpdates(false);
    }
  };

  const gourmetCard = (
    <View style={isWideLayout ? styles.desktopCard : undefined}>
      <Text style={styles.sectionTitle}>Gourmet Credentials</Text>

      <View style={styles.inputGroup}>
        <Text style={styles.label}>Username</Text>
        <TextInput
          style={styles.input}
          value={gUsername}
          onChangeText={setGUsername}
          placeholder="Enter username"
          placeholderTextColor={colors.textTertiary}
          autoCapitalize="none"
          autoCorrect={false}
        />
      </View>

      <View style={styles.inputGroup}>
        <Text style={styles.label}>Password</Text>
        <TextInput
          style={styles.input}
          value={gPassword}
          onChangeText={setGPassword}
          placeholder="Enter password"
          placeholderTextColor={colors.textTertiary}
          secureTextEntry
          autoCapitalize="none"
          autoCorrect={false}
        />
      </View>

      <View style={styles.buttonRow}>
        <Pressable
          style={[styles.button, styles.buttonSecondary]}
          onPress={handleGourmetTest}
          disabled={gTestStatus === 'testing'}
        >
          <Text style={styles.buttonSecondaryText}>
            {gTestStatus === 'testing' ? 'Testing...' : 'Test Connection'}
          </Text>
        </Pressable>

        <Pressable
          style={[styles.button, styles.buttonPrimary]}
          onPress={handleGourmetSave}
          disabled={gSaving}
        >
          <Text style={styles.buttonPrimaryText}>
            {gSaving ? 'Saving...' : 'Save'}
          </Text>
        </Pressable>
      </View>

      {gTestStatus === 'success' && (
        <View style={styles.statusSuccess}>
          <Text style={styles.statusSuccessText}>
            Connected as {userInfo?.username}
          </Text>
        </View>
      )}

      {gTestStatus === 'error' && (
        <View style={styles.statusError}>
          <Text style={styles.statusErrorText}>
            {gourmetError || 'Connection failed'}
          </Text>
        </View>
      )}

      {gourmetStatus === 'authenticated' && (
        <View style={styles.sessionSection}>
          <Text style={styles.sessionInfo}>
            Logged in as: {userInfo?.username}
          </Text>
          <Pressable style={[styles.button, styles.buttonDanger]} onPress={handleGourmetLogout}>
            <Text style={styles.buttonDangerText}>Logout</Text>
          </Pressable>
        </View>
      )}
    </View>
  );

  const ventopayCard = (
    <View style={isWideLayout ? styles.desktopCard : undefined}>
      {!isWideLayout && <View style={styles.divider} />}
      <Text style={styles.sectionTitle}>Ventopay Credentials</Text>
      <Text style={styles.sectionSubtitle}>For vending machines and POS billing</Text>

      <View style={styles.inputGroup}>
        <Text style={styles.label}>Username</Text>
        <TextInput
          style={styles.input}
          value={vUsername}
          onChangeText={setVUsername}
          placeholder="Enter username"
          placeholderTextColor={colors.textTertiary}
          autoCapitalize="none"
          autoCorrect={false}
        />
      </View>

      <View style={styles.inputGroup}>
        <Text style={styles.label}>Password</Text>
        <TextInput
          style={styles.input}
          value={vPassword}
          onChangeText={setVPassword}
          placeholder="Enter password"
          placeholderTextColor={colors.textTertiary}
          secureTextEntry
          autoCapitalize="none"
          autoCorrect={false}
        />
      </View>

      <View style={styles.buttonRow}>
        <Pressable
          style={[styles.button, styles.buttonSecondary]}
          onPress={handleVentopayTest}
          disabled={vTestStatus === 'testing'}
        >
          <Text style={styles.buttonSecondaryText}>
            {vTestStatus === 'testing' ? 'Testing...' : 'Test Connection'}
          </Text>
        </Pressable>

        <Pressable
          style={[styles.button, styles.buttonPrimary]}
          onPress={handleVentopaySave}
          disabled={vSaving}
        >
          <Text style={styles.buttonPrimaryText}>
            {vSaving ? 'Saving...' : 'Save'}
          </Text>
        </Pressable>
      </View>

      {vTestStatus === 'success' && (
        <View style={styles.statusSuccess}>
          <Text style={styles.statusSuccessText}>Connected to Ventopay</Text>
        </View>
      )}

      {vTestStatus === 'error' && (
        <View style={styles.statusError}>
          <Text style={styles.statusErrorText}>
            {ventopayError || 'Connection failed'}
          </Text>
        </View>
      )}

      {ventopayStatus === 'authenticated' && (
        <View style={styles.sessionSection}>
          <Text style={styles.sessionInfo}>Ventopay session active</Text>
          <Pressable style={[styles.button, styles.buttonDanger]} onPress={handleVentopayLogout}>
            <Text style={styles.buttonDangerText}>Logout</Text>
          </Pressable>
        </View>
      )}
    </View>
  );

  const appearanceCard = (
    <View style={isWideLayout ? styles.desktopCard : styles.appearanceSection}>
      {!isWideLayout && <View style={styles.divider} />}
      <Text style={styles.sectionTitle}>Appearance</Text>
      <View style={styles.themeRow}>
        {THEME_OPTIONS.map((opt) => (
          <Pressable
            key={opt.value}
            style={[
              styles.themeOption,
              themePreference === opt.value && styles.themeOptionActive,
            ]}
            onPress={() => setThemePreference(opt.value)}
          >
            <Text
              style={[
                styles.themeOptionText,
                themePreference === opt.value && styles.themeOptionTextActive,
              ]}
            >
              {opt.label}
            </Text>
          </Pressable>
        ))}
      </View>
    </View>
  );

  const updatesCard = isDesktop() ? (
    <View style={isWideLayout ? styles.desktopCard : undefined}>
      {!isWideLayout && <View style={styles.divider} />}
      <Text style={styles.sectionTitle}>Updates</Text>
      <Pressable
        style={[styles.button, styles.buttonSecondary]}
        onPress={handleCheckForUpdates}
        disabled={checkingUpdates}
      >
        <Text style={styles.buttonSecondaryText}>
          {checkingUpdates ? 'Checking...' : 'Check for Updates'}
        </Text>
      </Pressable>
    </View>
  ) : null;

  return (
    <KeyboardAvoidingView style={{ flex: 1 }} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
    <ScrollView style={[styles.container, { paddingTop: insets.top }]} contentContainerStyle={isWideLayout ? styles.contentDesktop : styles.content} keyboardShouldPersistTaps="handled">
      {isWideLayout ? (
        <>
          <View style={styles.desktopRow}>
            {gourmetCard}
            {ventopayCard}
          </View>
          <View style={styles.desktopRow}>
            {appearanceCard}
            {updatesCard}
          </View>
        </>
      ) : (
        <>
          {gourmetCard}
          {ventopayCard}
          {appearanceCard}
          {updatesCard}
        </>
      )}
    </ScrollView>
    </KeyboardAvoidingView>
  );
}

const createStyles = (c: Colors) =>
  StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: c.background,
    },
    content: {
      padding: 20,
      paddingBottom: 100,
    },
    contentDesktop: {
      padding: 24,
      paddingBottom: 40,
      maxWidth: 900,
      alignSelf: 'center' as const,
      width: '100%',
    },
    desktopRow: {
      flexDirection: 'row' as const,
      gap: 20,
      marginBottom: 20,
    },
    desktopCard: {
      flex: 1,
      padding: 20,
      ...cardSurface(c),
    },
    sectionTitle: {
      fontSize: 22,
      fontWeight: '600',
      color: c.textPrimary,
      marginBottom: 16,
    },
    sectionSubtitle: {
      fontSize: 13,
      color: c.textTertiary,
      marginTop: -12,
      marginBottom: 16,
    },
    divider: {
      height: 1,
      backgroundColor: c.glassHighlight,
      marginVertical: 24,
    },
    inputGroup: {
      marginBottom: 16,
    },
    label: {
      fontSize: 13,
      fontWeight: '600',
      color: c.textSecondary,
      marginBottom: 6,
    },
    input: {
      fontSize: 15,
      color: c.textPrimary,
      ...inputField(c),
    },
    buttonRow: {
      flexDirection: 'row',
      gap: 12,
      marginTop: 8,
    },
    button: {
      flex: 1,
      paddingVertical: 14,
      borderRadius: 14,
      alignItems: 'center',
    },
    buttonPrimary: {
      ...buttonPrimary(c),
    },
    buttonPrimaryText: {
      color: '#fff',
      fontWeight: '700',
      fontSize: 15,
    },
    buttonSecondary: {
      ...buttonSecondary(c),
    },
    buttonSecondaryText: {
      color: c.primary,
      fontWeight: '700',
      fontSize: 15,
    },
    buttonDanger: {
      flex: 0,
      paddingHorizontal: 24,
      ...buttonDanger(c),
    },
    buttonDangerText: {
      color: '#fff',
      fontWeight: '700',
      fontSize: 15,
    },
    statusSuccess: {
      padding: 14,
      marginTop: 16,
      ...tintedBanner(c, c.glassSuccess),
    },
    statusSuccessText: {
      color: c.successText,
      fontWeight: '600',
    },
    statusError: {
      padding: 14,
      marginTop: 16,
      ...tintedBanner(c, c.glassError),
    },
    statusErrorText: {
      color: c.errorText,
      fontWeight: '600',
    },
    sessionSection: {
      marginTop: 16,
    },
    sessionInfo: {
      fontSize: 14,
      color: c.textSecondary,
      marginBottom: 12,
    },
    appearanceSection: {
      marginTop: 32,
    },
    themeRow: {
      flexDirection: 'row',
      gap: 10,
    },
    themeOption: {
      flex: 1,
      paddingVertical: 12,
      alignItems: 'center',
      ...bannerSurface(c),
    },
    themeOptionActive: {
      backgroundColor: Platform.OS === 'android' ? c.primarySurface : c.glassPrimary,
      borderColor: Platform.OS === 'android' ? c.primary : undefined,
      borderBottomColor: c.primary,
    },
    themeOptionText: {
      fontSize: 14,
      fontWeight: '600',
      color: c.textSecondary,
    },
    themeOptionTextActive: {
      color: c.primary,
    },
  });
