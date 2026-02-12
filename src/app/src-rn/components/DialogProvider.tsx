import { createContext, useCallback, useContext, useMemo, useRef, useState } from 'react';
import { Modal, Pressable, StyleSheet, Text, View } from 'react-native';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';

interface DialogButton {
  label: string;
  style?: 'default' | 'destructive' | 'cancel';
}

interface DialogConfig {
  title: string;
  message?: string;
  buttons: DialogButton[];
}

interface DialogContextValue {
  /** Show a confirmation dialog. Resolves with the index of the tapped button. */
  showDialog: (config: DialogConfig) => Promise<number>;
  /** Shorthand: info/error alert with a single OK button. */
  alert: (title: string, message?: string) => Promise<void>;
  /** Shorthand: destructive confirmation. Resolves true if confirmed. */
  confirm: (title: string, message: string, confirmLabel?: string, cancelLabel?: string) => Promise<boolean>;
}

const DialogContext = createContext<DialogContextValue | null>(null);

export function useDialog(): DialogContextValue {
  const ctx = useContext(DialogContext);
  if (!ctx) throw new Error('useDialog must be used within DialogProvider');
  return ctx;
}

export function DialogProvider({ children }: { children: React.ReactNode }) {
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);

  const [visible, setVisible] = useState(false);
  const [config, setConfig] = useState<DialogConfig>({ title: '', buttons: [] });
  const resolveRef = useRef<((index: number) => void) | null>(null);

  const showDialog = useCallback((cfg: DialogConfig): Promise<number> => {
    return new Promise((resolve) => {
      resolveRef.current = resolve;
      setConfig(cfg);
      setVisible(true);
    });
  }, []);

  const handlePress = useCallback((index: number) => {
    setVisible(false);
    resolveRef.current?.(index);
    resolveRef.current = null;
  }, []);

  const alert = useCallback(
    async (title: string, message?: string) => {
      await showDialog({ title, message, buttons: [{ label: 'OK' }] });
    },
    [showDialog]
  );

  const confirm = useCallback(
    async (title: string, message: string, confirmLabel = 'Bestätigen', cancelLabel = 'Abbrechen') => {
      const idx = await showDialog({
        title,
        message,
        buttons: [
          { label: cancelLabel, style: 'cancel' },
          { label: confirmLabel, style: 'destructive' },
        ],
      });
      return idx === 1;
    },
    [showDialog]
  );

  const value = useMemo(() => ({ showDialog, alert, confirm }), [showDialog, alert, confirm]);

  return (
    <DialogContext.Provider value={value}>
      {children}
      <Modal
        visible={visible}
        transparent
        animationType="fade"
        onRequestClose={() => handlePress(0)}
      >
        <Pressable style={styles.backdrop} onPress={() => handlePress(0)}>
          <Pressable style={styles.dialog} onPress={() => { /* prevent dismiss */ }}>
            <Text style={styles.title}>{config.title}</Text>
            {config.message ? (
              <Text style={styles.message}>{config.message}</Text>
            ) : null}
            <View style={styles.buttonRow}>
              {config.buttons.map((btn, i) => (
                <Pressable
                  key={i}
                  style={[
                    styles.button,
                    btn.style === 'destructive' && styles.buttonDestructive,
                    btn.style === 'cancel' && styles.buttonCancel,
                    config.buttons.length === 1 && styles.buttonSingle,
                  ]}
                  onPress={() => handlePress(i)}
                >
                  <Text
                    style={[
                      styles.buttonText,
                      btn.style === 'destructive' && styles.buttonTextDestructive,
                      btn.style === 'cancel' && styles.buttonTextCancel,
                    ]}
                  >
                    {btn.label}
                  </Text>
                </Pressable>
              ))}
            </View>
          </Pressable>
        </Pressable>
      </Modal>
    </DialogContext.Provider>
  );
}

const createStyles = (c: Colors) =>
  StyleSheet.create({
    backdrop: {
      flex: 1,
      backgroundColor: c.overlay,
      justifyContent: 'center',
      alignItems: 'center',
      padding: 40,
    },
    dialog: {
      backgroundColor: c.surface,
      borderRadius: 14,
      padding: 24,
      maxWidth: 340,
      width: '100%',
      borderWidth: 0.5,
      borderColor: c.border,
    },
    title: {
      fontSize: 17,
      fontWeight: '600',
      color: c.textPrimary,
      textAlign: 'center',
    },
    message: {
      fontSize: 14,
      color: c.textSecondary,
      textAlign: 'center',
      marginTop: 8,
      lineHeight: 20,
    },
    buttonRow: {
      flexDirection: 'row',
      marginTop: 20,
      gap: 10,
    },
    button: {
      flex: 1,
      paddingVertical: 10,
      borderRadius: 8,
      alignItems: 'center',
      backgroundColor: c.primary,
    },
    buttonSingle: {
      flex: undefined,
      paddingHorizontal: 32,
      alignSelf: 'center',
    },
    buttonDestructive: {
      backgroundColor: c.error,
    },
    buttonCancel: {
      backgroundColor: 'transparent',
      borderWidth: 1,
      borderColor: c.border,
    },
    buttonText: {
      fontSize: 15,
      fontWeight: '600',
      color: '#fff',
    },
    buttonTextDestructive: {
      color: '#fff',
    },
    buttonTextCancel: {
      color: c.textSecondary,
    },
  });
