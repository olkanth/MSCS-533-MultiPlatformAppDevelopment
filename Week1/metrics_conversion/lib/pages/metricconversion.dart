import 'package:flutter/material.dart';

class MetricConversion extends StatefulWidget {
  const MetricConversion({super.key});

  @override
  State<MetricConversion> createState() => _MetricConversionState();
}

class _MetricConversionState extends State<MetricConversion> {
  String _fromUnit = 'Meters';
  String _toUnit = 'Kilometers';

  final Map<String, double> _conversionRates = {
    'Meters': 1.0,
    'Kilometers': 1000.0,
    'Feet': 0.3048,
    'Miles': 1609.34,
    'Inches': 0.0254,
    'Centimeters': 0.01,
    'Yards': 0.9144,
  };
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Metric Conversion.."),
        centerTitle: true,
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            Text(
              "Value",
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodyLarge,
            ),
            const SizedBox(height: 20),
            TextField(
              keyboardType: TextInputType.number,
              decoration: const InputDecoration(
                labelText: 'Enter value',
                border: OutlineInputBorder(),
              ),
            ),

            const SizedBox(height: 20),
            Text(
              "From",
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodyLarge,
            ),
            const SizedBox(height: 20),
            DropdownButton(
              items: _conversionRates.keys.map<DropdownMenuItem<String>>((
                String value,
              ) {
                return DropdownMenuItem<String>(
                  value: value,
                  child: Text(value),
                );
              }).toList(),
              onChanged: (String? value) {
                setState(() {
                      _fromUnit = value!;
                    });
              },
              value: _fromUnit,
              isExpanded: true,
              hint: const Text('From Unit'),
            
            ),
           const SizedBox(height: 20),
           Text(
              "To",
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodyLarge,
            ),
          DropdownButton(
              items: _conversionRates.keys.map<DropdownMenuItem<String>>((
                String value,
              ) {
                return DropdownMenuItem<String>(
                  value: value,
                  child: Text(value),
                );
              }).toList(),
              onChanged: (String? value) {
                setState(() {
                      _toUnit = value!;
                    });
              },
              value: _toUnit,
              isExpanded: true,
              hint: const Text('To Unit'),
            ),
            const SizedBox(height: 20),
            ElevatedButton(
              onPressed: () {  },
              child: const Text('Convert'),
              style: ElevatedButton.styleFrom(
                backgroundColor: Theme.of(context).colorScheme.primary,
                foregroundColor: Theme.of(context).colorScheme.onPrimary,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
