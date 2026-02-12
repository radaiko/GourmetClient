import { Link, Stack } from 'expo-router';
import { View, Text, StyleSheet } from 'react-native';

export default function NotFoundScreen() {
  return (
    <>
      <Stack.Screen options={{ title: 'Hoppla!' }} />
      <View style={styles.container}>
        <Text>Diese Seite existiert nicht.</Text>
        <Link href="/" style={styles.link}>
          <Text>Zur Startseite!</Text>
        </Link>
      </View>
    </>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 20 },
  link: { marginTop: 15, paddingVertical: 15 },
});
