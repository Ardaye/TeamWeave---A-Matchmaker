import mongoose from 'mongoose';
import dotenv from 'dotenv';

dotenv.config();

let mongoServer = null;

export const connectDB = async () => {
  const uri = process.env.MONGODB_URI || 'mongodb://127.0.0.1:27017/teamweave';
  
  try {
    console.log(`Connecting to MongoDB at: ${uri}...`);
    await mongoose.connect(uri, {
      serverSelectionTimeoutMS: 2000,
    });
    console.log('MongoDB connected successfully to primary instance.');
  } catch (err) {
    console.warn(`Could not connect to local/remote MongoDB (${err.message}). Starting MongoMemoryServer for development fallback...`);
    try {
      const { MongoMemoryServer } = await import('mongodb-memory-server');
      mongoServer = await MongoMemoryServer.create();
      const inMemoryUri = mongoServer.getUri();
      console.log(`Connected to in-memory MongoDB instance at: ${inMemoryUri}`);
      await mongoose.connect(inMemoryUri);
    } catch (memErr) {
      console.error('Failed to initialize in-memory MongoDB fallback:', memErr);
      throw memErr;
    }
  }
};

export const disconnectDB = async () => {
  await mongoose.disconnect();
  if (mongoServer) {
    await mongoServer.stop();
  }
};
